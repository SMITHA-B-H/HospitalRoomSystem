using HospitalRoomAPI.Repositories;
using HospitalRoomAPI.Models;
using HospitalRoomAPI.Models.Common;
using HospitalRoomAPI.DTOs;
using Microsoft.AspNetCore.Http;

namespace HospitalRoomAPI.Services
{
    public class AnnouncementService : IAnnouncementService
    {
        private readonly IAnnouncementRepository _repo;
        private readonly IDisplayService _display;
        private readonly IConfiguration _config;

        public AnnouncementService(
            IAnnouncementRepository repo,
            IDisplayService display,
            IConfiguration config)
        {
            _repo = repo;
            _display = display;
            _config = config;
        }

        private string GetRootPath() => _config["StoragePath"]!;

        // ================= FILE DELETE =================
        private void DeleteFile(string? url)
        {
            if (string.IsNullOrEmpty(url)) return;

            try
            {
                var relativePath = url.Replace("/files/", "");
                var fullPath = Path.Combine(GetRootPath(), relativePath);

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    Console.WriteLine("Deleted: " + fullPath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Delete error: " + ex.Message);
            }
        }

        // ================= CREATE =================
        public async Task<ApiResponse<PatientAnnouncement>> CreateAsync(PatientAnnouncement model, int hospitalId)
        {
            var ist = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

            model.CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, ist);
            model.IsActive = true;
            model.HospitalId = hospitalId;

            if (model.ExpiryHours > 0)
                model.ExpiryTime = model.CreatedAt.AddHours(model.ExpiryHours);

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
            {
                return new ApiResponse<PatientAnnouncement>
                {
                    Success = false,
                    Message = "Not found"
                };
            }

            DeleteFile(announcement.PosterUrl);
            DeleteFile(announcement.VideoUrl);

            await _repo.DeleteAsync(announcement);
            await _repo.SaveAsync();

            await PushUpdate(announcement);

            return new ApiResponse<PatientAnnouncement>
            {
                Success = true,
                Data = announcement
            };
        }

        // ================= AUTO DELETE EXPIRED =================
        public async Task RemoveExpiredAnnouncements()
        {
            var now = DateTime.Now;

            var expired = await _repo.GetExpiredAnnouncements(now);

            foreach (var a in expired)
            {
                DeleteFile(a.PosterUrl);
                DeleteFile(a.VideoUrl);

                await _repo.DeleteAsync(a);
            }

            await _repo.SaveAsync();
        }

        // ================= DELETE BY PATIENT =================
        public async Task DeleteByPatient(int patientId)
        {
            var list = await _repo.GetByPatientId(patientId);

            foreach (var a in list)
            {
                DeleteFile(a.PosterUrl);
                DeleteFile(a.VideoUrl);

                await _repo.DeleteAsync(a);
            }

            await _repo.SaveAsync();
        }

        // ================= FILE UPLOAD =================
        public async Task<ApiResponse<string>> UploadPoster(IFormFile file, int announcementId)
        {
            var folder = Path.Combine(GetRootPath(), "announcements", "posters");

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            var fileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(folder, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            var url = $"/files/announcements/posters/{fileName}";

            var announcement = await _repo.GetByIdAsync(announcementId);
            if (announcement == null)
                return new ApiResponse<string> { Success = false };

            announcement.PosterUrl = url;
            await _repo.SaveAsync();

            await PushUpdate(announcement);

            return new ApiResponse<string> { Success = true, Data = url };
        }

        public async Task<ApiResponse<string>> UploadVideo(IFormFile file, int announcementId)
        {
            var folder = Path.Combine(GetRootPath(), "announcements", "videos");

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            var fileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(folder, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            var url = $"/files/announcements/videos/{fileName}";

            var announcement = await _repo.GetByIdAsync(announcementId);
            if (announcement == null)
                return new ApiResponse<string> { Success = false };

            announcement.VideoUrl = url;
            await _repo.SaveAsync();

            await PushUpdate(announcement);

            return new ApiResponse<string> { Success = true, Data = url };
        }

        // ================= FIXED INTERFACE METHODS =================

        public async Task<ApiResponse<List<PatientAnnouncement>>> GetRoomAsync(int roomId)
        {
            var data = await _repo.GetRoomAnnouncements(roomId);

            return new ApiResponse<List<PatientAnnouncement>>
            {
                Success = true,
                Data = data
            };
        }

        public async Task<ApiResponse<List<PatientAnnouncement>>> GetAllAsync()
        {
            var data = await _repo.GetAllActiveAnnouncements();

            return new ApiResponse<List<PatientAnnouncement>>
            {
                Success = true,
                Data = data
            };
        }

        public async Task<ApiResponse<PatientAnnouncement>> DeactivateAsync(int id)
        {
            var announcement = await _repo.GetByIdAsync(id);

            if (announcement == null)
            {
                return new ApiResponse<PatientAnnouncement>
                {
                    Success = false,
                    Message = "Not found"
                };
            }

            announcement.IsActive = false;

            await _repo.SaveAsync();

            return new ApiResponse<PatientAnnouncement>
            {
                Success = true,
                Data = announcement,
                Message = "Deactivated"
            };
        }

        public async Task<ApiResponse<List<PatientDto>>> GetPatientsByRoom(int roomId)
        {
            var data = await _repo.GetPatientsByRoom(roomId);

            return new ApiResponse<List<PatientDto>>
            {
                Success = true,
                Data = data
            };
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
    }
}