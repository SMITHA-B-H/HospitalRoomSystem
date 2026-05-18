using HospitalRoomAPI.Repositories;
using HospitalRoomAPI.DTOs;
using HospitalRoomAPI.Data;
using HospitalRoomAPI.Models;
using HospitalRoomAPI.Models.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace HospitalRoomAPI.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly ISettingsRepository _repo;
        private readonly IDisplayService _display;
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public SettingsService(
            ISettingsRepository repo,
            IDisplayService display,
            AppDbContext context,
            IConfiguration config)
        {
            _repo = repo;
            _display = display;
            _context = context;
            _config = config;
        }

        // ================= HELPER =================
        private string GetRootPath()
        {
            return _config["StoragePath"]!;
        }

        private void DeleteLocalFile(string? url)
        {
            if (string.IsNullOrEmpty(url))
                return;

            try
            {
                var relativePath = url.Replace("/files/", "");
                var fullPath = Path.Combine(GetRootPath(), relativePath);

                if (File.Exists(fullPath))
                    File.Delete(fullPath);
            }
            catch
            {
                // optional logging
            }
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
            existing.ShowClock = dto.ShowClock;

            await _repo.SaveSettings(existing);

            var rooms = await _repo.GetRoomNumbers(null, null, hospitalId);

            // ? FIX: sequential execution (NO Task.WhenAll)
            foreach (var room in rooms)
            {
                await _display.PushRoomUpdate(room);
            }

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
            if (file == null || file.Length == 0)
            {
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = "File not selected"
                };
            }

            var folder = Path.Combine(GetRootPath(), "settings", "logos");

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            var fileName = $"{hospitalId}_{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(folder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var url = $"/files/settings/logos/{fileName}";

            var existing = await _repo.GetSettings(hospitalId, null, null);
            if (!string.IsNullOrEmpty(existing?.LogoUrl))
            {
                DeleteLocalFile(existing.LogoUrl);
            }

            await _repo.SaveLogo(url, hospitalId);

            return new ApiResponse<string>
            {
                Success = true,
                Data = url
            };
        }

        // ================= UPLOAD VIDEO =================
        public async Task<ApiResponse<string>> UploadVideo(UploadVideoDto dto, int hospitalId)
        {
            if (dto.File == null || dto.File.Length == 0)
            {
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = "File not selected"
                };
            }

            var folder = Path.Combine(GetRootPath(), "settings", "videos");

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            var fileName = $"{hospitalId}_{Guid.NewGuid()}_{dto.File.FileName}";
            var filePath = Path.Combine(folder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await dto.File.CopyToAsync(stream);
            }

            var url = $"/files/settings/videos/{fileName}";

            await _repo.SaveVideo(url, hospitalId, dto);

            var rooms = await _repo.GetRoomNumbers(null, null, hospitalId);

            // ? FIX: sequential execution
            foreach (var room in rooms)
            {
                await _display.PushRoomUpdate(room);
            }

            return new ApiResponse<string>
            {
                Success = true,
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

            try
            {
                // ================= STEP 1: CLEAN PATH =================
                string cleanPath = path;

                // ? if full URL ? convert to relative
                if (cleanPath.StartsWith("http"))
                {
                    var uri = new Uri(cleanPath);
                    cleanPath = uri.AbsolutePath; // /files/settings/videos/abc.mp4
                }

                // ? decode URL (handles %20 spaces)
                cleanPath = Uri.UnescapeDataString(cleanPath);

                // ================= STEP 2: BUILD FILE PATH =================
                // remove "/files/"
                var relativePath = cleanPath.Replace("/files/", "");

                // build physical path
                var fullPath = Path.Combine(GetRootPath(), relativePath);

                Console.WriteLine("DELETE REQUEST PATH: " + path);
                Console.WriteLine("RELATIVE PATH: " + relativePath);
                Console.WriteLine("FULL FILE PATH: " + fullPath);

                // ================= STEP 3: DELETE FILE =================
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    Console.WriteLine("FILE DELETED SUCCESSFULLY ?");
                }
                else
                {
                    Console.WriteLine("FILE NOT FOUND ?");
                }

                // ================= STEP 4: DELETE DB =================
                await _repo.DeleteVideo(cleanPath);

                // ================= STEP 5: PUSH UPDATES =================
                var rooms = await _repo.GetRoomNumbers(null, null, hospitalId);

                foreach (var room in rooms)
                {
                    await _display.PushRoomUpdate(room);
                }

                return new ApiResponse<object>
                {
                    Success = true
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine("DELETE ERROR: " + ex.Message);

                return new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        // ================= GET SETTINGS =================
        public async Task<ApiResponse<object>> GetSettingsByHospital(int hospitalId)
        {
            var settings = await _repo.GetSettings(hospitalId, null, null);
            var videos = await _repo.GetVideos(hospitalId, null, null);
            var announcements = await _repo.GetAnnouncements(hospitalId, null, null);

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

        public async Task<object>
        GetPublicSettingsAsync()
        {
            return await
                _repo
                    .GetPublicSettingsAsync();
        }
    }
}