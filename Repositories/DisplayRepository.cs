using Microsoft.EntityFrameworkCore;
using HospitalRoomAPI.Data;
using HospitalRoomAPI.Models;
using HospitalRoomAPI.DTOs;

namespace HospitalRoomAPI.Repositories
{
    public class DisplayRepository : IDisplayRepository
    {
        private readonly AppDbContext _context;

        public DisplayRepository(AppDbContext context)
        {
            _context = context;
        }

        // ================= ROOM WITH DETAILS =================
        public async Task<Room?>
        GetRoomWithDetailsAsync(
        string roomNumber)
        {
            return await _context.Rooms

                .AsNoTracking()

                .Include(r => r.Floor)

                .Include(r => r.Beds)

                    .ThenInclude(b => b.Patient)

                        .ThenInclude(p => p.Doctor)

                .FirstOrDefaultAsync(

                    r => r.RoomNumber == roomNumber
                );
        }

        // ================= SETTINGS =================
        public async Task<Setting?> GetSettingsAsync(int hospitalId, int roomId, int floorId)
        {
            return await _context.Settings.FirstOrDefaultAsync(s =>
                s.HospitalId == hospitalId &&
                (s.RoomId == roomId || s.RoomId == null) &&
                (s.FloorId == floorId || s.FloorId == null));
        }

        // ================= ADS VIDEOS =================
        public async Task<List<string>> GetVideosAsync(int hospitalId, int roomId, int floorId)
        {
            var videos = await _context.AdsVideos
                .Where(v =>
                    v.HospitalId == hospitalId &&
                    (v.RoomId == null || v.RoomId == roomId) &&
                    (v.FloorId == null || v.FloorId == floorId))
                .Select(v => v.FilePath) // ✅ ONLY DB FIELD (EF SAFE)
                .ToListAsync();

            // ✅ Convert AFTER DB call
            return videos;
        }

        // ================= ANNOUNCEMENTS =================
        public async Task<List<AnnouncementDisplayDto>> GetAnnouncementsAsync(int hospitalId, int roomId, int floorId)
        {
            var data = await _context.PatientAnnouncements
                .Where(a =>
                    a.IsActive &&
                    (a.ExpiryTime == null || a.ExpiryTime > DateTime.UtcNow) &&
                    a.HospitalId == hospitalId &&
                    (
                        a.RoomId == roomId ||
                        (a.RoomId == null && a.FloorId == floorId) ||
                        (a.RoomId == null && a.FloorId == null)
                    )
                )
                .OrderByDescending(a => a.CreatedAt)
                .Take(5)
                .Select(a => new AnnouncementDisplayDto
                {
                    Id = a.Id,
                    PatientName = a.PatientName,
                    Message = a.Message,
                    CreatedAt = a.CreatedAt,
                    ExpiryTime = a.ExpiryTime,
                    PosterUrl = a.PosterUrl,
                    VideoUrl = a.VideoUrl,
                    IsActive = a.IsActive,
                    ExpiryHours = a.ExpiryHours
                })
                .ToListAsync();

            // ✅ Fix URLs AFTER DB call
            

            return data;
        }

        // ================= ROOM SCOPE =================
        public async Task<List<string>> GetRoomNumbersByScope(int? roomId, int? floorId, int hospitalId)
        {
            if (roomId != null)
            {
                return await _context.Rooms
                    .Where(r => r.Id == roomId)
                    .Select(r => r.RoomNumber)
                    .ToListAsync();
            }

            if (floorId != null)
            {
                return await _context.Rooms
                    .Where(r => r.FloorId == floorId)
                    .Select(r => r.RoomNumber)
                    .ToListAsync();
            }

            return await _context.Rooms
                .Include(r => r.Floor)
                .Where(r => r.Floor.HospitalId == hospitalId)
                .Select(r => r.RoomNumber)
                .ToListAsync();
        }


    }
}