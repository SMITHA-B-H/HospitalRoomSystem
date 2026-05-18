using HospitalRoomAPI.Models;
using HospitalRoomAPI.DTOs;

namespace HospitalRoomAPI.Repositories
{
    public interface IAnnouncementRepository
    {
        Task<PatientAnnouncement?> GetByIdAsync(int id);
        Task AddAsync(PatientAnnouncement announcement);
        Task DeleteAsync(PatientAnnouncement announcement);
        Task<List<PatientAnnouncement>> GetExpiredAnnouncements(DateTime now);
        Task<List<PatientAnnouncement>> GetByPatientId(int patientId);

        Task<List<string>> GetRoomNumbersByRoomId(int roomId);
        Task<List<string>> GetRoomNumbersByFloorId(int floorId);
        Task<List<string>> GetRoomNumbersByHospitalId(int hospitalId);

        Task<List<PatientAnnouncement>> GetRoomAnnouncements(int roomId);
        Task<List<PatientAnnouncement>> GetAllActiveAnnouncements();

        Task<List<PatientDto>> GetPatientsByRoom(int roomId);

        Task SaveAsync();

        Task<Patient?> GetPatientById(int patientId);
    }
}