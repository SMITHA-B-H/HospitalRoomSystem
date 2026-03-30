using HospitalRoomAPI.Models;

namespace HospitalRoomAPI.Repositories
{
    public interface IFloorRepository
    {
        Task<List<Floor>> GetFloorsAsync(int hospitalId);
        Task<Floor?> GetFloorByIdAsync(int id);
        Task AddFloorAsync(Floor floor);
        Task DeleteFloorAsync(Floor floor);
        Task<List<string>> GetRoomNumbersByFloorIdAsync(int floorId);
        Task SaveChangesAsync();
    }
}