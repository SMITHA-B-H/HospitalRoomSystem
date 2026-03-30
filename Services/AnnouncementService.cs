using HospitalRoomAPI.Repositories;
using HospitalRoomAPI.Models;
using HospitalRoomAPI.Models.Common;

namespace HospitalRoomAPI.Services
{
    public class AnnouncementService : IAnnouncementService
    {
        private readonly IAnnouncementRepository _repo;
        private readonly IDisplayService _display;

        public AnnouncementService(IAnnouncementRepository repo, IDisplayService display)
        {
            _repo = repo;
            _display = display;
        }

        public async Task<ApiResponse<object>> CreateAsync(PatientAnnouncement model, int hospitalId)
        {
            model.CreatedAt = DateTime.UtcNow;
            model.IsActive = true;
            model.HospitalId = hospitalId;

            if (model.ExpiryHours > 0)
                model.ExpiryTime = DateTime.UtcNow.AddHours(model.ExpiryHours);

            if (model.PatientId != null)
            {
                var patient = await _repo.GetPatientById(model.PatientId.Value);
                if (patient != null)
                    model.PatientName = patient.Name;
            }

            await _repo.AddAsync(model);
            await _repo.SaveAsync();

            return new ApiResponse<object>
            {
                Success = true,
                Data = model.Id
            };
        }

        public async Task<ApiResponse<object>> DeleteAsync(int id)
        {
            var announcement = await _repo.GetByIdAsync(id);
            if (announcement == null)
                return new ApiResponse<object> { Success = false };

            await _repo.DeleteAsync(announcement);
            await _repo.SaveAsync();

            await PushUpdate(announcement);

            return new ApiResponse<object> { Success = true };
        }

        public async Task<ApiResponse<object>> DeactivateAsync(int id)
        {
            var announcement = await _repo.GetByIdAsync(id);
            if (announcement == null)
                return new ApiResponse<object> { Success = false };

            announcement.IsActive = false;
            await _repo.SaveAsync();

            await PushUpdate(announcement);

            return new ApiResponse<object> { Success = true };
        }

        public async Task<ApiResponse<List<PatientAnnouncement>>> GetRoomAsync(int roomId)
        {
            var data = await _repo.GetRoomAnnouncements(roomId);
            return new ApiResponse<List<PatientAnnouncement>> { Success = true, Data = data };
        }

        public async Task<ApiResponse<List<PatientAnnouncement>>> GetAllAsync()
        {
            var data = await _repo.GetAllActiveAnnouncements();
            return new ApiResponse<List<PatientAnnouncement>> { Success = true, Data = data };
        }

        public async Task<ApiResponse<object>> GetPatientsByRoom(int roomId)
        {
            var data = await _repo.GetPatientsByRoom(roomId);
            return new ApiResponse<object> { Success = true, Data = data };
        }

        // ================= PUSH =================
        private async Task PushUpdate(PatientAnnouncement a)
        {
            List<string> rooms;

            if (a.RoomId != null)
                rooms = await _repo.GetRoomNumbersByRoomId(a.RoomId.Value);
            else if (a.FloorId != null)
                rooms = await _repo.GetRoomNumbersByFloorId(a.FloorId.Value);
            else
                rooms = await _repo.GetRoomNumbersByHospitalId(a.HospitalId ?? 0);

            foreach (var room in rooms)
                await _display.PushRoomUpdate(room);
        }
    }
}