using HospitalRoomAPI.Repositories;
using HospitalRoomAPI.DTOs;
using HospitalRoomAPI.Models;
using HospitalRoomAPI.Models.Common;
using Microsoft.AspNetCore.Http;

namespace HospitalRoomAPI.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly ISettingsRepository _repo;
        private readonly IDisplayService _display;

        public SettingsService(ISettingsRepository repo, IDisplayService display)
        {
            _repo = repo;
            _display = display;
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

            // 🔥 Optimized push (parallel)
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
            var path = await _repo.UploadLogo(file, hospitalId);

            return new ApiResponse<string>
            {
                Success = !string.IsNullOrEmpty(path),
                Data = path
            };
        }

        // ================= UPLOAD VIDEO =================

        public async Task<ApiResponse<string>> UploadVideo(UploadVideoDto dto, int hospitalId)
        {
            var path = await _repo.UploadVideo(dto, hospitalId);

            var rooms = await _repo.GetRoomNumbers(null, null, hospitalId);

            foreach (var room in rooms)
            {
                await _display.PushRoomUpdate(room);
            }

            return new ApiResponse<string>
            {
                Success = !string.IsNullOrEmpty(path),
                Data = path
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

            await _repo.DeleteVideo(path);

            var rooms = await _repo.GetRoomNumbers(null, null, hospitalId);

            // 🔥 Optimized push
            var tasks = rooms.Select(r => _display.PushRoomUpdate(r));
            await Task.WhenAll(tasks);

            return new ApiResponse<object>
            {
                Success = true
            };
        }
    }
}