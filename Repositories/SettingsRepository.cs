using Microsoft.EntityFrameworkCore;
using HospitalRoomAPI.Data;
using HospitalRoomAPI.Models;
using HospitalRoomAPI.DTOs;

namespace HospitalRoomAPI.Repositories
{
    public class SettingsRepository : ISettingsRepository
    {
        private readonly AppDbContext _context;

        public SettingsRepository(AppDbContext context)
        {
            _context = context;
        }

        // ================= ROOM =================

        public async Task<Room?> GetRoomWithDetails(int roomId)
        {
            return await _context.Rooms
                .Include(r => r.Floor)
                    .ThenInclude(f => f.Hospital)
                .Include(r => r.Beds)
                    .ThenInclude(b => b.Patient)
                        .ThenInclude(p => p.Doctor)
                .FirstOrDefaultAsync(r => r.Id == roomId);
        }

        // ================= SETTINGS =================

        public async Task<Setting?> GetSettings(int hospitalId, int? roomId, int? floorId)
        {
            return await _context.Settings.FirstOrDefaultAsync(s =>
                s.HospitalId == hospitalId &&
                s.RoomId == roomId &&
                s.FloorId == floorId);
        }

        public async Task<Setting> SaveSettings(Setting settings)
        {
            if (settings.Id == 0)
                _context.Settings.Add(settings);
            else
                _context.Settings.Update(settings);

            await _context.SaveChangesAsync();
            return settings;
        }

        // ================= ROOM NUMBERS =================

        public async Task<List<string>> GetRoomNumbers(int? roomId, int? floorId, int hospitalId)
        {
            if (roomId != null)
            {
                return await _context.Rooms
                    .Where(r => r.Id == roomId)
                    .Select(r => r.RoomNumber!)
                    .ToListAsync();
            }

            if (floorId != null)
            {
                return await _context.Rooms
                    .Where(r => r.FloorId == floorId)
                    .Select(r => r.RoomNumber!)
                    .ToListAsync();
            }

            return await _context.Rooms
                .Where(r => r.Floor != null && r.Floor.HospitalId == hospitalId)
                .Select(r => r.RoomNumber!)
                .ToListAsync();
        }

        // ================= VIDEOS =================

        public async Task<List<string>> GetVideos(int hospitalId, int? roomId, int? floorId)
        {
            return await _context.AdsVideos
                .Where(v =>
                    v.HospitalId == hospitalId &&
                    (v.RoomId == null || v.RoomId == roomId) &&
                    (v.FloorId == null || v.FloorId == floorId))
                .Select(v => v.FilePath!)
                .ToListAsync();
        }

        // ================= ANNOUNCEMENTS =================

        public async Task<List<PatientAnnouncement>> GetAnnouncements(int hospitalId, int? floorId, int? roomId)
        {
            return await _context.PatientAnnouncements
                .Where(a =>
                    a.IsActive &&
                    (a.ExpiryTime == null || a.ExpiryTime > DateTime.UtcNow) &&
                    a.HospitalId == hospitalId &&
                    (floorId == null || a.FloorId == floorId) &&
                    (roomId == null || a.RoomId == roomId))
                .ToListAsync();
        }

        // ================= FILE UPLOADS =================

        public async Task<string> UploadLogo(IFormFile file, int hospitalId)
        {
            var fileName = $"{Guid.NewGuid()}_{file.FileName}";
            var path = Path.Combine("wwwroot/logos", fileName);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/logos/{fileName}";
        }

        public async Task<string> UploadVideo(UploadVideoDto dto, int hospitalId)
        {
            var fileName = $"{Guid.NewGuid()}_{dto.File.FileName}";
            var path = Path.Combine("wwwroot/videos", fileName);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await dto.File.CopyToAsync(stream);
            }

            var video = new AdsVideo
            {
                FileName = fileName,
                FilePath = $"/videos/{fileName}",
                HospitalId = hospitalId
            };

            _context.AdsVideos.Add(video);
            await _context.SaveChangesAsync();

            return video.FilePath;
        }

        public async Task DeleteVideo(string path)
        {
            var video = await _context.AdsVideos
                .FirstOrDefaultAsync(v => v.FilePath == path);

            if (video != null)
            {
                _context.AdsVideos.Remove(video);
                await _context.SaveChangesAsync();
            }
        }
    }
}