using HospitalRoomAPI.Repositories;
using System.Linq;

namespace HospitalRoomAPI.Services
{
    public class DeviceService : IDeviceService
    {
        private readonly IDeviceRepository _repository;

        public DeviceService(IDeviceRepository repository)
        {
            _repository = repository;
        }

        public object GetDevices()
        {
            var devices = _repository.GetAllDevices();

            return devices.Select(d => new
            {
                d.DeviceName,
                d.RoomNumber,
                d.IsOnline,
                d.LastSeen
            }).ToList();
        }
    }
}