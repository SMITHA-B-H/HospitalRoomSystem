using HospitalRoomAPI.Models;

namespace HospitalRoomAPI.Repositories
{
    public interface IAnnouncementRepository
    {
        Task<PatientAnnouncement?> GetByIdAsync(int id);
        Task AddAsync(PatientAnnouncement announcement);
        Task DeleteAsync(PatientAnnouncement announcement);

        Task<List<string>> GetRoomNumbersByRoomId(int roomId);
        Task<List<string>> GetRoomNumbersByFloorId(int floorId);
        Task<List<string>> GetRoomNumbersByHospitalId(int hospitalId);

        Task<List<PatientAnnouncement>> GetRoomAnnouncements(int roomId);
        Task<List<PatientAnnouncement>> GetAllActiveAnnouncements();

        Task<List<object>> GetPatientsByRoom(int roomId);

        Task SaveAsync();

        Task<Patient?> GetPatientById(int patientId);
    }
}