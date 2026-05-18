using System.Threading.Tasks;
using HospitalRoomAPI.Models;
namespace HospitalRoomAPI.Services
{
    public interface IDisplayService
    {
        Task<RoomDisplay?> BuildRoomDisplay(string roomNumber);
        Task PushRoomUpdate(string roomNumber);
        Task PushRoomUpdateByScope(int? roomId, int? floorId, int hospitalId);
    }
}