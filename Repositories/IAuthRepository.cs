using HospitalRoomAPI.Models;

namespace HospitalRoomAPI.Repositories
{
    public interface IAuthRepository
    {
        Task<User?> GetUserByEmailAsync(string email);
        Task<Hospital> AddHospitalAsync(Hospital hospital);
        Task AddUserAsync(User user);
        Task SaveChangesAsync();
        Task<Hospital?> GetHospitalByIdAsync(int id);
    }
}