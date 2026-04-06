using HospitalRoomAPI.Repositories;
using HospitalRoomAPI.Models;
using HospitalRoomAPI.Models.Common;
using Microsoft.AspNetCore.Http;

namespace HospitalRoomAPI.Services
{
    public class AnnouncementService : IAnnouncementService
    {
        private readonly IAnnouncementRepository _repo;
        private readonly IDisplayService _display;
        private readonly IFileStorageService _fileService;

        public AnnouncementService(
            IAnnouncementRepository repo,
            IDisplayService display,
            IFileStorageService fileService)
        {
            _repo = repo;
            _display = display;
            _fileService = fileService;
        }

        // ================= CREATE =================
        public async Task<ApiResponse<PatientAnnouncement>> CreateAsync(PatientAnnouncement model, int hospitalId)
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

            await PushUpdate(model);

            return new ApiResponse<PatientAnnouncement>
            {
                Success = true,
                Data = model
            };
        }

        // ================= DELETE =================
        public async Task<ApiResponse<PatientAnnouncement>> DeleteAsync(int id)
        {
            var announcement = await _repo.GetByIdAsync(id);
            if (announcement == null)
                return new ApiResponse<PatientAnnouncement> { Success = false, Message = "Not found" };

            await _repo.DeleteAsync(announcement);
            await _repo.SaveAsync();

            await PushUpdate(announcement);

            return new ApiResponse<PatientAnnouncement>
            {
                Success = true,
                Data = announcement
            };
        }

        // ================= DEACTIVATE =================
        public async Task<ApiResponse<PatientAnnouncement>> DeactivateAsync(int id)
        {
            var announcement = await _repo.GetByIdAsync(id);
            if (announcement == null)
                return new ApiResponse<PatientAnnouncement> { Success = false, Message = "Not found" };

            announcement.IsActive = false;
            await _repo.SaveAsync();

            await PushUpdate(announcement);

            return new ApiResponse<PatientAnnouncement> { Success = true, Data = announcement };
        }

        // ================= GET ROOM =================
        public async Task<ApiResponse<List<PatientAnnouncement>>> GetRoomAsync(int roomId)
        {
            var data = await _repo.GetRoomAnnouncements(roomId);
            return new ApiResponse<List<PatientAnnouncement>> { Success = true, Data = data };
        }

        // ================= GET ALL =================
        public async Task<ApiResponse<List<PatientAnnouncement>>> GetAllAsync()
        {
            var data = await _repo.GetAllActiveAnnouncements();
            return new ApiResponse<List<PatientAnnouncement>> { Success = true, Data = data };
        }

        // ================= GET PATIENTS =================
        public async Task<ApiResponse<List<Patient>>> GetPatientsByRoom(int roomId)
        {
            var data = await _repo.GetPatientsByRoom(roomId);
            return new ApiResponse<List<Patient>> { Success = true, Data = data.Cast<Patient>().ToList() };
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
            {
                if (!string.IsNullOrEmpty(room))
                    await _display.PushRoomUpdate(room);
            }
        }

        // ================= FILE UPLOAD =================
        public async Task<ApiResponse<string>> UploadPoster(IFormFile file)
        {
            var url = await _fileService.UploadAsync(file);
            return new ApiResponse<string> { Success = true, Data = url };
        }

        public async Task<ApiResponse<string>> UploadVideo(IFormFile file)
        {
            var url = await _fileService.UploadAsync(file);
            return new ApiResponse<string> { Success = true, Data = url };
        }
    }
}