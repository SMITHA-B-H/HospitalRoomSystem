using Microsoft.AspNetCore.Mvc;
using HospitalRoomAPI.Services;

namespace HospitalRoomAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DeviceController : ControllerBase
    {
        private readonly IDeviceService _service;

        public DeviceController(IDeviceService service)
        {
            _service = service;
        }

        [HttpGet("devices")]
        public IActionResult GetDevices()
        {
            var result = _service.GetDevices();
            return Ok(result);
        }
    }
}