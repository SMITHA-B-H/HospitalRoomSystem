using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HospitalRoomAPI.Models;
using HospitalRoomAPI.Services;
using Microsoft.AspNetCore.Http;
using HospitalRoomAPI.DTOs;

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
            int hospitalId = 1; // TODO: replace with user claims
            return Ok(await _service.CreateAsync(model, hospitalId));
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


        [Authorize]
        [HttpPost("upload-announcement-poster")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadPoster([FromForm] UploadAnnouncementDto model)
        {
            var result = await _service.UploadPoster(model.File, model.AnnouncementId);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [Authorize]
        [HttpPost("upload-announcement-video")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadVideo([FromForm] UploadAnnouncementDto model)
        {
            var result = await _service.UploadVideo(model.File, model.AnnouncementId);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }


    }
}