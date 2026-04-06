using HospitalRoomAPI.Models;
using HospitalRoomAPI.Models.Common;

namespace HospitalRoomAPI.Services
{
    public interface IAnnouncementService
    {
        Task<ApiResponse<PatientAnnouncement>> CreateAsync(PatientAnnouncement model, int hospitalId);

        Task<ApiResponse<List<PatientAnnouncement>>> GetRoomAsync(int roomId);

        Task<ApiResponse<List<PatientAnnouncement>>> GetAllAsync();

        Task<ApiResponse<PatientAnnouncement>> DeleteAsync(int id);

        Task<ApiResponse<PatientAnnouncement>> DeactivateAsync(int id);

        Task<ApiResponse<List<Patient>>> GetPatientsByRoom(int roomId);// or DTO if you have one
        Task<ApiResponse<string>> UploadPoster(IFormFile file);
        Task<ApiResponse<string>> UploadVideo(IFormFile file);
    }
}