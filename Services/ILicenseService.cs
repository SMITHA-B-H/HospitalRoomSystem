using System.Threading.Tasks;

namespace HospitalRoomAPI.Services
{
    public interface ILicenseService
    {
        Task<bool> ValidateAsync();
    }
}