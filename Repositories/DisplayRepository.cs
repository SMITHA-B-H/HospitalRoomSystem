using Microsoft.EntityFrameworkCore;
using HospitalRoomAPI.Data;
using HospitalRoomAPI.Models;

namespace HospitalRoomAPI.Repositories
{
    public class DisplayRepository : IDisplayRepository
    {
        private readonly AppDbContext _context;

        public DisplayRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Room?> GetRoomWithDetailsAsync(string roomNumber)
        {
            return await _context.Rooms
                .Include(r => r.Beds)
                    .ThenInclude(b => b.Patient)
                        .ThenInclude(p => p.Doctor)
                .Include(r => r.Floor)
                    .ThenInclude(f => f.Hospital)
                .FirstOrDefaultAsync(r => r.RoomNumber == roomNumber);
        }

        public async Task<Setting?> GetSettingsAsync(int hospitalId, int roomId, int floorId)
        {
            return await _context.Settings.FirstOrDefaultAsync(s =>
                s.HospitalId == hospitalId &&
                (s.RoomId == roomId || s.RoomId == null) &&
                (s.FloorId == floorId || s.FloorId == null));
        }

        public async Task<List<string>> GetVideosAsync(int hospitalId, int roomId, int floorId)
        {
            return await _context.AdsVideos
                .Where(v =>
                    v.HospitalId == hospitalId &&
                    (v.RoomId == null || v.RoomId == roomId) &&
                    (v.FloorId == null || v.FloorId == floorId))
                .Select(v => Path.GetFileName(v.FilePath))
                .ToListAsync();
        }

        public async Task<List<object>> GetAnnouncementsAsync(int hospitalId, int roomId, int floorId)
        {
            return await _context.PatientAnnouncements
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
                .Select(a => new
                {
                    a.Id,
                    a.PatientName,
                    a.Message,
                    a.CreatedAt,
                    a.ExpiryTime,
                    a.PosterUrl,
                    a.VideoUrl,
                    a.IsActive,
                    a.ExpiryHours
                })
                .ToListAsync<object>();
        }

        // ✅ IMPLEMENT MISSING METHOD
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