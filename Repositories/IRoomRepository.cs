using HospitalRoomAPI.Models;

namespace HospitalRoomAPI.Repositories
{
    public interface IRoomRepository
    {
        Task<List<Room>> GetRoomsAsync(string role, int hospitalId, int? floorId);
        Task<Floor?> GetFloorByIdAsync(int floorId, int hospitalId);
        Task<Room?> GetRoomByIdWithBedsAsync(int id);
        Task AddRoomAsync(Room room);
        Task<Bed?> GetBedByIdAsync(int bedId);
        Task AddPatientAsync(Patient patient);
        Task RemoveAnnouncementsByPatientIdAsync(int patientId);
        Task<List<object>> GetRoomsByFloorAsync(int floorId);
        Task RemoveRoomAsync(Room room);
        Task RemovePatientByIdAsync(int patientId);
        Task SaveChangesAsync();
    }
}
