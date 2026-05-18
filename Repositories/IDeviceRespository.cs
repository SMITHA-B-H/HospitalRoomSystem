using HospitalRoomAPI.Models;

namespace HospitalRoomAPI.Repositories
{
    public interface IDeviceRepository
    {
        List<DisplayDevice> GetAllDevices();
    }
}