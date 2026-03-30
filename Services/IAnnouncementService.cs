using HospitalRoomAPI.Models;
using HospitalRoomAPI.Models.Common;

namespace HospitalRoomAPI.Services
{
    public interface IAnnouncementService
    {
        Task<ApiResponse<object>> CreateAsync(PatientAnnouncement model, int hospitalId);
        Task<ApiResponse<object>> DeleteAsync(int id);
        Task<ApiResponse<object>> DeactivateAsync(int id);
        Task<ApiResponse<List<PatientAnnouncement>>> GetRoomAsync(int roomId);
        Task<ApiResponse<List<PatientAnnouncement>>> GetAllAsync();
        Task<ApiResponse<object>> GetPatientsByRoom(int roomId);
    }
}