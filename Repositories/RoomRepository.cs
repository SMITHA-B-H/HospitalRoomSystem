using HospitalRoomAPI.Data;
using HospitalRoomAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace HospitalRoomAPI.Repositories
{
    public class RoomRepository : IRoomRepository
    {
        private readonly AppDbContext _context;

        public RoomRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Room>> GetRoomsAsync(string role, int hospitalId, int? floorId)
        {
            IQueryable<Room> query = _context.Rooms
                .Include(r => r.Floor)
                .Include(r => r.Beds)
                    .ThenInclude(b => b.Patient)
                        .ThenInclude(p => p.Doctor);

            if (role == "SuperAdmin")
            {
                query = query.Where(r => r.Floor.HospitalId == hospitalId);
            }
            else if (role == "FloorAdmin" && floorId.HasValue)
            {
                query = query.Where(r =>
                    r.Floor.HospitalId == hospitalId &&
                    r.FloorId == floorId.Value
                );
            }

            return await query.ToListAsync();
        }

        public async Task<Floor?> GetFloorByIdAsync(int floorId, int hospitalId)
        {
            return await _context.Floors
                .FirstOrDefaultAsync(f => f.Id == floorId && f.HospitalId == hospitalId);
        }

        public async Task<Room?> GetRoomByIdWithBedsAsync(int id)
        {
            return await _context.Rooms
                .Include(r => r.Floor)
                .Include(r => r.Beds)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task AddRoomAsync(Room room)
        {
            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();
        }

        public async Task<Bed?> GetBedByIdAsync(int bedId)
        {
            return await _context.Beds
                .Include(b => b.Room)
                .Include(b => b.Patient)
                .FirstOrDefaultAsync(b => b.Id == bedId);
        }

        public async Task AddPatientAsync(Patient patient)
        {
            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveAnnouncementsByPatientIdAsync(int patientId)
        {
            var announcements = await _context.PatientAnnouncements
                .Where(a => a.PatientId == patientId)
                .ToListAsync();

            if (announcements.Any())
            {
                _context.PatientAnnouncements.RemoveRange(announcements);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<List<object>> GetRoomsByFloorAsync(int floorId)
        {
            return await _context.Rooms
                .Where(r => r.FloorId == floorId)
                .Select(r => new
                {
                    id = r.Id,
                    roomNumber = r.RoomNumber
                })
                .Cast<object>()
                .ToListAsync();
        }

        public async Task RemoveRoomAsync(Room room)
        {
            _context.Beds.RemoveRange(room.Beds ?? new List<Bed>());
            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();
        }

        public async Task RemovePatientByIdAsync(int patientId)
        {
            var patient = await _context.Patients.FindAsync(patientId);
            if (patient != null)
            {
                _context.Patients.Remove(patient);
                await _context.SaveChangesAsync();
            }
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
     
        }

        public async Task<Patient?> GetPatientByIdAsync(int patientId)
        {
            return await _context.Patients
                .FirstOrDefaultAsync(p => p.Id == patientId);
        }
    }
}
