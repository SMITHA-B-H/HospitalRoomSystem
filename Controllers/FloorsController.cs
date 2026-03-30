using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using HospitalRoomAPI.Services;
using HospitalRoomAPI.DTOs;

namespace HospitalRoomAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class FloorsController : ControllerBase
    {
        private readonly IFloorService _service;

        public FloorsController(IFloorService service)
        {
            _service = service;
        }

        private int GetHospitalId()
        {
            return int.Parse(User.FindFirst("HospitalId")!.Value);
        }

        [HttpGet]
        public async Task<IActionResult> GetFloors()
        {
            var result = await _service.GetFloorsAsync(GetHospitalId());
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> AddFloor(CreateFloorDto dto)
        {
            var result = await _service.AddFloorAsync(dto, GetHospitalId());
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFloor(int id, UpdateFloorDto dto)
        {
            var result = await _service.UpdateFloorAsync(id, dto);
            if (!result.Success) return NotFound(result);

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFloor(int id)
        {
            var result = await _service.DeleteFloorAsync(id);
            if (!result.Success) return NotFound(result);

            return Ok(result);
        }
    }
}