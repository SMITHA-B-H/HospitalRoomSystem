using HospitalRoomAPI.Models;
using HospitalRoomAPI.Models.Common;
using HospitalRoomAPI.DTOs;

namespace HospitalRoomAPI.Services
{
    public interface IAnnouncementService
    {
        Task<ApiResponse<PatientAnnouncement>> CreateAsync(PatientAnnouncement model, int hospitalId);

        Task<ApiResponse<List<PatientAnnouncement>>> GetRoomAsync(int roomId);

        Task<ApiResponse<List<PatientAnnouncement>>> GetAllAsync();

        Task<ApiResponse<PatientAnnouncement>> DeleteAsync(int id);

        Task<ApiResponse<PatientAnnouncement>> DeactivateAsync(int id);

        Task<ApiResponse<List<PatientDto>>> GetPatientsByRoom(int roomId);// or DTO if you have one

        Task RemoveExpiredAnnouncements();

        Task DeleteByPatient(int patientId);

        Task<ApiResponse<string>> UploadPoster(IFormFile file, int announcementId);
        Task<ApiResponse<string>> UploadVideo(IFormFile file, int announcementId);
    }
}