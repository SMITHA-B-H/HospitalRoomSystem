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

                    if (existingRoom != null)
                    {
                        return new ApiResponse<Room>
                        {
                            Success = false,
                            Message = "Room already exists."
                        };
                    }

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
                return new ApiResponse<Patient>
                {
                    Success = false,
                    Message = "Bed not found"
                };

            if (bed.Status == "Occupied")
            {
                return new ApiResponse<Patient>
                {
                    Success = false,
                    Message = "Bed already occupied"
                };
            }

            var patient = new Patient
            {
                Name = dto.Name,
                Age = dto.Age,
                DoctorId = dto.DoctorId,
                PatientType = dto.PatientType
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

        // ================= BOOK BED =================

        public async Task<ApiResponse<object>> BookBedAsync(int bedId)
        {
            var bed = await _repository.GetBedByIdAsync(bedId);

            if (bed == null)
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Bed not found"
                };
            }

            if (bed.Status == "Occupied")
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Bed already occupied"
                };
            }

            if (bed.Status == "Booked")
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Bed already booked"
                };
            }

            bed.Status = "Booked";

            await _repository.SaveChangesAsync();

            await _displayService.PushRoomUpdate(bed.Room!.RoomNumber);

            return new ApiResponse<object>
            {
                Success = true,
                Message = "Bed booked successfully"
            };
        }

        // ================= CANCEL BOOKING =================

        public async Task<ApiResponse<object>> CancelBookingAsync(int bedId)
        {
            var bed = await _repository.GetBedByIdAsync(bedId);

            if (bed == null)
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Bed not found"
                };
            }

            if (bed.Status != "Booked")
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Bed is not booked"
                };
            }

            bed.Status = "Available";

            await _repository.SaveChangesAsync();

            await _displayService.PushRoomUpdate(bed.Room!.RoomNumber);

            return new ApiResponse<object>
            {
                Success = true,
                Message = "Booking cancelled"
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
            {
                return new ApiResponse<Room>
                {
                    Success = false,
                    Message = "Room not found"
                };
            }

            // ======================================
            // PREVENT NULL COLLECTION ERRORS
            // ======================================

            updatedRoom.Beds ??= new List<Bed>();
            room.Beds ??= new List<Bed>();

            // ======================================
            // DUPLICATE ROOM NUMBER CHECK
            // ======================================

            var rooms = await _repository.GetRoomsAsync(
                role,
                hospitalId,
                floorId
            ) ?? new List<Room>();

            var duplicateRoom = rooms.FirstOrDefault(r =>
                r.Id != id &&
                r.RoomNumber != null &&
                updatedRoom.RoomNumber != null &&
                r.RoomNumber.ToLower() ==
                updatedRoom.RoomNumber.ToLower()
            );

            if (duplicateRoom != null)
            {
                return new ApiResponse<Room>
                {
                    Success = false,
                    Message = "Room already exists."
                };
            }

            // ==============================
            // BLOCK EDIT IF PATIENT EXISTS
            // ==============================

            bool hasPatients = room.Beds.Any(
                b => b.Status == "Occupied"
            );

            if (hasPatients)
            {
                return new ApiResponse<Room>
                {
                    Success = false,
                    Message = "Cannot edit room while patients are admitted."
                };
            }

            // ==============================
            // UPDATE ROOM
            // ==============================

            room.RoomNumber = updatedRoom.RoomNumber;
            room.RoomName = updatedRoom.RoomName;
            room.RoomType = updatedRoom.RoomType;
            room.TotalBeds = updatedRoom.TotalBeds;

            // ==============================
            // BED MANAGEMENT
            // ==============================

            var currentBedsCount = room.Beds.Count;

            // ==============================
            // UPDATE EXISTING BED STATUS
            // ==============================

            foreach (var existingBed in room.Beds)
            {
                var updatedBed = updatedRoom.Beds
                    .FirstOrDefault(b => b.Id == existingBed.Id);

                if (updatedBed != null)
                {
                    existingBed.Status = updatedBed.Status;
                }
            }

            // ==============================
            // ADD BEDS
            // ==============================

            if (updatedRoom.TotalBeds > currentBedsCount)
            {
                for (int i = currentBedsCount + 1; i <= updatedRoom.TotalBeds; i++)
                {
                    room.Beds.Add(new Bed
                    {
                        BedNumber = i,
                        Status = "Available"
                    });
                }
            }

            // ==============================
            // REMOVE BEDS
            // ==============================

            if (updatedRoom.TotalBeds < currentBedsCount)
            {
                var bedsToRemove = room.Beds
                    .Where(b => b.BedNumber > updatedRoom.TotalBeds)
                    .ToList();

                foreach (var bed in bedsToRemove)
                {
                    room.Beds.Remove(bed);
                }
            }

            await _repository.SaveChangesAsync();

            await _displayService.PushRoomUpdate(room.RoomNumber!);

            return new ApiResponse<Room>
            {
                Success = true,
                Data = room,
                Message = "Room updated successfully"
            };
        }

        public async Task<ApiResponse<object>> UpdatePatientAsync(int patientId,UpdatePatientDto dto)
        {
            var patient = await _repository.GetPatientByIdAsync(patientId);

            if (patient == null)
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Patient not found"
                };
            }

            patient.Name = dto.Name;
            patient.Age = dto.Age;

            if (dto.DoctorId.HasValue)
            {
                patient.DoctorId = dto.DoctorId.Value;
            }

            patient.PatientType = dto.PatientType;

            await _repository.SaveChangesAsync();

            return new ApiResponse<object>
            {
                Success = true,
                Message = "Patient updated successfully"
            };
        }

    }
}