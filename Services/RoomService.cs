using HospitalRoomAPI.Models;
using HospitalRoomAPI.Models.Common;
using HospitalRoomAPI.DTOs;
using HospitalRoomAPI.Repositories;

namespace HospitalRoomAPI.Services
{
    public class RoomService : IRoomService
    {
        private readonly IRoomRepository _repository;
        private readonly IDisplayService _displayService;

        private readonly IAnnouncementService _announcementService;

        public RoomService(
            IRoomRepository repository,
            IDisplayService displayService,
            IAnnouncementService announcementService)
        {
            _repository = repository;
            _displayService = displayService;
            _announcementService = announcementService;
        }

        // ================= GET ROOMS =================
        public async Task<ApiResponse<object>> GetRoomsAsync(string role, int hospitalId, int? floorId)
        {
            var rooms = await _repository.GetRoomsAsync(role, hospitalId, floorId);

            return new ApiResponse<object>
            {
                Success = true,
                Data = rooms
            };
        }

        // ================= CREATE ROOM =================
        public async Task<ApiResponse<Room>> CreateRoomAsync(
            Room room,
            string role,
            int hospitalId,
            int? floorId)
        {

            var rooms = await _repository.GetRoomsAsync(role, hospitalId, floorId)
            ?? new List<Room>();

            var existingRoom = rooms
                .FirstOrDefault(r =>
                    r.RoomNumber != null &&
                    room.RoomNumber != null &&
                    r.RoomNumber.ToLower() == room.RoomNumber.ToLower());

            // FIX 1: Floor override (your test expects this)
            if (role == "FloorAdmin" && floorId.HasValue)
            {
                room.FloorId = floorId.Value;
            }

            // FIX 2: Always initialize beds (avoid null exception)
            room.Beds = new List<Bed>();

            for (int i = 1; i <= room.TotalBeds; i++)
            {
                room.Beds.Add(new Bed
                {
                    BedNumber = i,
                    Status = "Available"
                });
            }

            await _repository.AddRoomAsync(room);

            // FIX 3: IMPORTANT → SaveChanges (your test expects it)
            await _repository.SaveChangesAsync();

            // FIX 4: Always call SignalR
            if (!string.IsNullOrEmpty(room.RoomNumber))
            {
                await _displayService.PushRoomUpdate(room.RoomNumber);
            }

            return new ApiResponse<Room>
            {
                Success = true,
                Data = room
            };
        }

        // ================= ASSIGN PATIENT =================
        public async Task<ApiResponse<Patient>> AssignPatientAsync(AssignPatientDto dto)
        {
            var bed = await _repository.GetBedByIdAsync(dto.BedId);

            if (bed == null)
                return new ApiResponse<Patient> { Success = false, Message = "Bed not found" };

            var patient = new Patient
            {
                Name = dto.Name,
                Age = dto.Age,
                DoctorId = dto.DoctorId
            };

            await _repository.AddPatientAsync(patient);

            bed.PatientId = patient.Id;
            bed.Status = "Occupied";

            await _repository.SaveChangesAsync();

            await _displayService.PushRoomUpdate(bed.Room!.RoomNumber);

            return new ApiResponse<Patient>
            {
                Success = true,
                Data = patient
            };
        }

        // ================= DISCHARGE =================
        public async Task<ApiResponse<object>> DischargePatientAsync(int bedId)
        {
            var bed = await _repository.GetBedByIdAsync(bedId);

            if (bed == null || bed.Patient == null)
                return new ApiResponse<object> { Success = false, Message = "Invalid request" };

            var patientId = bed.Patient.Id;

            // ✅ DELETE ANNOUNCEMENTS + MEDIA
            await _announcementService.DeleteByPatient(patientId);

            // ✅ DELETE PATIENT
            await _repository.RemovePatientByIdAsync(patientId);

            bed.PatientId = null;
            bed.Status = "Available";

            await _repository.SaveChangesAsync();

            await _displayService.PushRoomUpdate(bed.Room!.RoomNumber);

            return new ApiResponse<object>
            {
                Success = true,
                Message = "Patient discharged"
            };
        }

        // ================= GET BY FLOOR =================
        public async Task<List<object>> GetRoomsByFloorAsync(int floorId)
        {
            return await _repository.GetRoomsByFloorAsync(floorId);
        }


        // ================= DELETE =================
        public async Task<ApiResponse<object>> DeleteRoomAsync(
            int id,
            string role,
            int hospitalId,
            int? floorId)
        {
            var room = await _repository.GetRoomByIdWithBedsAsync(id);

            if (room == null)
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Room not found"
                };
            }

            // Only patient condition
            if (room.Beds != null &&
                room.Beds.Any(b => b.PatientId != null))
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Room cannot be deleted because patient is present."
                };
            }

            await _repository.RemoveRoomAsync(room);
            await _repository.SaveChangesAsync();

            await _displayService.PushRoomUpdate(room.RoomNumber!);

            return new ApiResponse<object>
            {
                Success = true,
                Message = "Room deleted successfully"
            };
        }

        // ================= UPDATE =================
        public async Task<ApiResponse<Room>> UpdateRoomAsync(
            int id,
            Room updatedRoom,
            string role,
            int hospitalId,
            int? floorId)
        {
            var room = await _repository.GetRoomByIdWithBedsAsync(id);

            if (room == null)
                return new ApiResponse<Room> { Success = false, Message = "Not found" };

            room.RoomNumber = updatedRoom.RoomNumber;
            room.RoomName = updatedRoom.RoomName;
            room.RoomType = updatedRoom.RoomType;
            room.TotalBeds = updatedRoom.TotalBeds;

            await _repository.SaveChangesAsync();

            await _displayService.PushRoomUpdate(room.RoomNumber!);

            return new ApiResponse<Room>
            {
                Success = true,
                Data = room
            };
        } 
    }
}