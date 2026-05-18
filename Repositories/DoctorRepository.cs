using Microsoft.EntityFrameworkCore;
using HospitalRoomAPI.Data;
using HospitalRoomAPI.Models;

namespace HospitalRoomAPI.Repositories
{
    public class DoctorRepository : IDoctorRepository
    {
        private readonly AppDbContext _context;

        public DoctorRepository(AppDbContext context)
        {
            _context = context;
        }

        // ================= GET =================
        public async Task<List<Doctor>> GetDoctorsAsync(int hospitalId)
        {
            return await _context.Doctors
                .Where(d => d.HospitalId == hospitalId)
                .AsNoTracking() // ?? performance + avoids stale tracking
                .ToListAsync();
        }

        // ================= GET BY ID =================
        public async Task<Doctor?> GetByIdAsync(int id)
        {
            return await _context.Doctors
                .FirstOrDefaultAsync(d => d.Id == id);
        }


        public async Task<Doctor?> GetByEmployeeIdAsync(string employeeId)
        {
            return await _context.Doctors
                .FirstOrDefaultAsync(x => x.EmployeeId == employeeId);
        }

        // ================= ADD =================
        public async Task AddAsync(Doctor doctor)
        {
            await _context.Doctors.AddAsync(doctor);
        }

        // ================= REMOVE =================
        public Task RemoveAsync(Doctor doctor)
        {
            _context.Doctors.Remove(doctor);
            return Task.CompletedTask;
        }

        // ================= SAVE =================
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        // ================= ROOM MAPPING =================
        public async Task<List<string>> GetDoctorRoomNumbersAsync(int doctorId)
        {
            return await _context.Beds
                .Include(b => b.Room)
                .Include(b => b.Patient)
                .Where(b => b.Patient != null && b.Patient.DoctorId == doctorId)
                .Select(b => b.Room != null ? b.Room.RoomNumber : null)
                .Where(r => r != null)
                .Distinct()
                .ToListAsync()!;
        }

        public async Task<bool>
UpdateDisplayAsync(
    int id,
    string displayNumber)
        {
            var doctor =
                await _context.Doctors
                .FindAsync(id);

            if (doctor == null)
                return false;

            doctor.DisplayNumber =
                displayNumber;

            await _context.SaveChangesAsync();

            return true;
        }
    }
}