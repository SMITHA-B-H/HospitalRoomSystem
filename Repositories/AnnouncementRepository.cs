using Microsoft.EntityFrameworkCore;
using HospitalRoomAPI.Data;
using HospitalRoomAPI.Models;
using HospitalRoomAPI.DTOs;

namespace HospitalRoomAPI.Repositories
{
    public class AnnouncementRepository : IAnnouncementRepository
    {
        private readonly AppDbContext _context;

        public AnnouncementRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(PatientAnnouncement announcement)
        {
            await _context.PatientAnnouncements.AddAsync(announcement);
        }

        public async Task<PatientAnnouncement?> GetByIdAsync(int id)
        {
            return await _context.PatientAnnouncements.FindAsync(id);
        }

        public async Task DeleteAsync(PatientAnnouncement announcement)
        {
            _context.PatientAnnouncements.Remove(announcement);
            await Task.CompletedTask;
        }

        public async Task<List<string>> GetRoomNumbersByRoomId(int roomId)
        {
            return await _context.Rooms
                .Where(r => r.Id == roomId)
                .Select(r => r.RoomNumber!)
                .ToListAsync();
        }

        public async Task<List<string>> GetRoomNumbersByFloorId(int floorId)
        {
            return await _context.Rooms
                .Where(r => r.FloorId == floorId)
                .Select(r => r.RoomNumber!)
                .ToListAsync();
        }

        public async Task<List<string>> GetRoomNumbersByHospitalId(int hospitalId)
        {
            return await _context.Rooms
                .Where(r => r.Floor.HospitalId == hospitalId)
                .Select(r => r.RoomNumber!)
                .ToListAsync();
        }

        public async Task<List<PatientAnnouncement>> GetRoomAnnouncements(int roomId)
        {
            var now = DateTime.Now;
            return await _context.PatientAnnouncements
                .Where(a =>
                    a.IsActive &&
                    (a.ExpiryTime == null || a.ExpiryTime > now) &&
                    (a.RoomId == roomId ||
                     a.FloorId == _context.Rooms.Where(r => r.Id == roomId).Select(r => r.FloorId).FirstOrDefault() ||
                     (a.RoomId == null && a.FloorId == null))
                )
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<PatientAnnouncement>> GetAllActiveAnnouncements()
        {
            var now = DateTime.Now;
            return await _context.PatientAnnouncements
                .Where(a => a.IsActive && (a.ExpiryTime == null || a.ExpiryTime > now))
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        

        public async Task<Patient?> GetPatientById(int patientId)
        {
            return await _context.Patients.FindAsync(patientId);
        }

        // ================= NEW METHODS =================

        public async Task<List<PatientAnnouncement>> GetExpiredAnnouncements(DateTime now)
        {
            return await _context.PatientAnnouncements
                .Where(x => x.ExpiryTime != null && x.ExpiryTime <= now)
                .ToListAsync();
        }

        public async Task<List<PatientAnnouncement>> GetByPatientId(int patientId)
        {
            return await _context.PatientAnnouncements
                .Where(x => x.PatientId == patientId)
                .ToListAsync();
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<List<PatientDto>> GetPatientsByRoom(int roomId)
        {
            return await _context.Beds
                .Include(b => b.Patient)
                .Where(b => b.RoomId == roomId && b.Patient != null)
                .Select(b => new PatientDto
                {
                    PatientId = b.Patient.Id,
                    Name = b.Patient.Name,
                    BedNumber = b.BedNumber
                })
                .ToListAsync();
        }
    }
}