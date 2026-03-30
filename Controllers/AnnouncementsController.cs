using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HospitalRoomAPI.Data;
using HospitalRoomAPI.Models;
using HospitalRoomAPI.Services;

namespace HospitalRoomAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnnouncementsController : ControllerBase
    {
        private readonly IAnnouncementService _service;

        public AnnouncementsController(IAnnouncementService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create(PatientAnnouncement model)
        {
            int hospitalId = 1; // get from claims
            var result = await _service.CreateAsync(model, hospitalId);
            return Ok(result);
        }

        [HttpGet("room/{roomId}")]
        public async Task<IActionResult> GetRoom(int roomId)
        {
            return Ok(await _service.GetRoomAsync(roomId));
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _service.GetAllAsync());
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            return Ok(await _service.DeleteAsync(id));
        }

        [HttpPut("deactivate/{id}")]
        public async Task<IActionResult> Deactivate(int id)
        {
            return Ok(await _service.DeactivateAsync(id));
        }

        [HttpGet("patients-by-room/{roomId}")]
        public async Task<IActionResult> GetPatients(int roomId)
        {
            return Ok(await _service.GetPatientsByRoom(roomId));
        }
    }
}