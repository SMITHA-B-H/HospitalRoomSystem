

using System.Threading.Tasks;

namespace HospitalRoomAPI.Services
{
    public interface IDisplayService
    {
        Task<object?> BuildRoomDisplay(string roomNumber);
        Task PushRoomUpdate(string roomNumber);
        Task PushRoomUpdateByScope(int? roomId, int? floorId, int hospitalId);
    }
}