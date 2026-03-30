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

        public async Task<List<Doctor>> GetDoctorsAsync(int hospitalId)
        {
            return await _context.Doctors
                .Where(d => d.HospitalId == hospitalId)
                .ToListAsync();
        }

        public async Task<Doctor?> GetByIdAsync(int id)
        {
            return await _context.Doctors.FindAsync(id);
        }

        public async Task AddAsync(Doctor doctor)
        {
            await _context.Doctors.AddAsync(doctor);
        }

        public async Task RemoveAsync(Doctor doctor)
        {
            _context.Doctors.Remove(doctor);
            await Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<List<string>> GetDoctorRoomNumbersAsync(int doctorId)
        {
            return await _context.Beds
                .Include(b => b.Room)
                .Include(b => b.Patient)
                .Where(b => b.Patient != null && b.Patient.DoctorId == doctorId)
                .Select(b => b.Room.RoomNumber)
                .Distinct()
                .ToListAsync();
        }
    }
}