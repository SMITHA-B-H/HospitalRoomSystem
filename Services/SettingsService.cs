using HospitalRoomAPI.Repositories;
using HospitalRoomAPI.DTOs;
using HospitalRoomAPI.Data;
using HospitalRoomAPI.Models;
using HospitalRoomAPI.Models.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;   // ✅ IMPORTANT

namespace HospitalRoomAPI.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly ISettingsRepository _repo;
        private readonly IDisplayService _display;
        private readonly IFileStorageService _fileService;
        private readonly AppDbContext _context;   // ✅ MOVED INSIDE CLASS

        public SettingsService(
            ISettingsRepository repo,
            IDisplayService display,
            IFileStorageService fileService,
            AppDbContext context)
        {
            _repo = repo;
            _display = display;
            _fileService = fileService;
            _context = context;   // ✅ ASSIGNED
        }

        // ================= SAVE SETTINGS =================
        public async Task<ApiResponse<object>> SaveSettings(SaveSettingsDto dto, int hospitalId)
        {
            var existing = await _repo.GetSettings(hospitalId, null, null);

            if (existing == null)
            {
                existing = new Setting
                {
                    HospitalId = hospitalId
                };
            }

            existing.ScrollingMessage = dto.ScrollingMessage;
            existing.AdsVolume = dto.AdsVolume;
            existing.ScrollingSpeed = dto.ScrollingSpeed;

            await _repo.SaveSettings(existing);

            var rooms = await _repo.GetRoomNumbers(null, null, hospitalId);

            var tasks = rooms.Select(r => _display.PushRoomUpdate(r));
            await Task.WhenAll(tasks);

            return new ApiResponse<object>
            {
                Success = true
            };
        }

        // ================= GET DISPLAY SETTINGS =================
        public async Task<ApiResponse<object>> GetDisplaySettings(int roomId)
        {
            var room = await _repo.GetRoomWithDetails(roomId);

            if (room == null)
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Room not found"
                };
            }

            var hospitalId = room.Floor?.HospitalId;

            if (hospitalId == null)
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid room configuration"
                };
            }

            var settings = await _repo.GetSettings(hospitalId.Value, roomId, null);
            var videos = await _repo.GetVideos(hospitalId.Value, roomId, null);
            var announcements = await _repo.GetAnnouncements(hospitalId.Value, roomId, null);

            return new ApiResponse<object>
            {
                Success = true,
                Data = new
                {
                    room.RoomNumber,
                    room.RoomName,
                    settings,
                    videos,
                    announcements
                }
            };
        }

        // ================= UPLOAD LOGO =================
        public async Task<ApiResponse<string>> UploadLogo(IFormFile file, int hospitalId)
        {
            var url = await _fileService.UploadAsync(file);

            await _repo.SaveLogo(url, hospitalId);

            return new ApiResponse<string>
            {
                Success = !string.IsNullOrEmpty(url),
                Data = url
            };
        }

        // ================= UPLOAD VIDEO =================
        public async Task<ApiResponse<string>> UploadVideo(UploadVideoDto dto, int hospitalId)
        {
            var url = await _fileService.UploadAsync(dto.File);

            await _repo.SaveVideo(url, hospitalId, dto);

            var rooms = await _repo.GetRoomNumbers(null, null, hospitalId);

            var tasks = rooms.Select(r => _display.PushRoomUpdate(r));
            await Task.WhenAll(tasks);

            return new ApiResponse<string>
            {
                Success = !string.IsNullOrEmpty(url),
                Data = url
            };
        }

        // ================= DELETE VIDEO =================
        public async Task<ApiResponse<object>> DeleteVideo(string path, int hospitalId)
        {
            if (string.IsNullOrEmpty(path))
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid path"
                };
            }

            var fileId = ExtractFileId(path);

            await _fileService.DeleteAsync(fileId);
            await _repo.DeleteVideo(path);

            var rooms = await _repo.GetRoomNumbers(null, null, hospitalId);

            var tasks = rooms.Select(r => _display.PushRoomUpdate(r));
            await Task.WhenAll(tasks);

            return new ApiResponse<object>
            {
                Success = true
            };
        }

        // ✅ ================= MAIN FIX =================
        public async Task<ApiResponse<object>> GetSettingsByHospital(int hospitalId)
        {
            var settings = await _repo.GetSettings(hospitalId, null, null);
            var videos = await _repo.GetVideos(hospitalId, null, null);
            var announcements = await _repo.GetAnnouncements(hospitalId, null, null);

            // ✅ GET HOSPITAL NAME FROM TABLE
            var hospital = await _context.Hospitals
                .Where(h => h.Id == hospitalId)
                .Select(h => new { h.Name })
                .FirstOrDefaultAsync();

            return new ApiResponse<object>
            {
                Success = true,
                Data = new
                {
                    hospitalName = hospital?.Name,
                    logoUrl = settings?.LogoUrl,
                    settings,
                    videos,
                    announcements
                }
            };
        }

        // ================= HELPER =================
        private string ExtractFileId(string url)
        {
            var uri = new Uri(url);
            var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
            return query["id"];
        }
    }
}