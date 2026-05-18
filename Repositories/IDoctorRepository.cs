using HospitalRoomAPI.Models;

namespace HospitalRoomAPI.Repositories
{
    public interface IDoctorRepository
    {
        Task<List<Doctor>> GetDoctorsAsync(int hospitalId);
        Task<Doctor?> GetByIdAsync(int id);
        Task AddAsync(Doctor doctor);
        Task RemoveAsync(Doctor doctor);
        Task SaveChangesAsync();
        Task<Doctor?> GetByEmployeeIdAsync(string employeeId);

        Task<List<string>> GetDoctorRoomNumbersAsync(int doctorId);

        Task<bool> UpdateDisplayAsync(int id,string displayNumber);
    }
}