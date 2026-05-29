using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using HospitalRoomAPI.Services;
using HospitalRoomAPI.Models;
using HospitalRoomAPI.DTOs;

namespace HospitalRoomAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class RoomsController : ControllerBase
    {
        private readonly IRoomService _roomService;

        public RoomsController(IRoomService roomService)
        {
            _roomService = roomService;
        }

        private int GetHospitalId()
        {
            var hospitalClaim = User.FindFirst("HospitalId");
            if (hospitalClaim == null)
                throw new Exception("HospitalId not found in token");

            return int.Parse(hospitalClaim.Value);
        }

        private int? GetFloorId()
        {
            var floorClaim = User.FindFirst("FloorId")?.Value;
            return string.IsNullOrEmpty(floorClaim) ? null : int.Parse(floorClaim);
        }

        // ================= GET ROOMS =================
        [HttpGet]
        public async Task<IActionResult> GetRooms()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            var hospitalId = GetHospitalId();
            var floorId = GetFloorId();

            var response = await _roomService.GetRoomsAsync(role!, hospitalId, floorId);
            return Ok(response);
        }

        // ================= CREATE ROOM =================
        [HttpPost]
        public async Task<IActionResult> CreateRoom(Room room)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            var hospitalId = GetHospitalId();
            var floorId = GetFloorId();

            var response = await _roomService.CreateRoomAsync(room, role!, hospitalId, floorId);
            if (!response.Success)
                //return Ok(new
                //{
                //    success = false,
                //    Message = "Room already exists."
                //});
                //return Ok(result);
                return Ok(response);

            return Ok(response);
        }

        // ================= ASSIGN PATIENT =================
        [HttpPost("assign-patient")]
        public async Task<IActionResult> AssignPatient(AssignPatientDto dto)
        {
            var response = await _roomService.AssignPatientAsync(dto);
            if (!response.Success)
                return BadRequest(response.Message);

            return Ok(response);
        }

        // ================= DISCHARGE =================
        [HttpPost("discharge")]
        public async Task<IActionResult> DischargePatient(int bedId)
        {
            var response = await _roomService.DischargePatientAsync(bedId);
            if (!response.Success)
                return BadRequest(response.Message);

            return Ok(response);
        }

        // ================= GET ROOMS BY FLOOR =================
        [HttpGet("by-floor/{floorId}")]
        public async Task<IActionResult> GetRoomsByFloor(int floorId)
        {
            var rooms = await _roomService.GetRoomsByFloorAsync(floorId);
            return Ok(rooms);
        }

        // ================= DELETE ROOM =================
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoom(int id)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            var hospitalId = GetHospitalId();
            var floorId = GetFloorId();

            var response = await _roomService.DeleteRoomAsync(id, role!, hospitalId, floorId);
            if (!response.Success)
                //return Ok(new
                //{
                //    success = false,
                //    message =
                //    "Room cannot be deleted because patient is present"
                //});
                //return Ok(result);
                return Ok(response);

            return Ok(response);
        }

        // ================= UPDATE ROOM =================
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRoom(int id, Room updatedRoom)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            var hospitalId = GetHospitalId();
            var floorId = GetFloorId();

            var response = await _roomService.UpdateRoomAsync(id, updatedRoom, role!, hospitalId, floorId);
            if (!response.Success)
                //return Ok(new
                //{
                //    success = false,
                //    message =
                //             "Cannot edit room while patients are present"
                //});
                //return Ok(result);
                return Ok(response);

            return Ok(response);
        }

        //================= BOOK BED =================
        [HttpPost("book-bed/{bedId}")]
        public async Task<IActionResult> BookBed(int bedId)
        {
            var result = await _roomService.BookBedAsync(bedId);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }


        //================= CANCEL BOOKING =================
        [HttpPost("cancel-booking/{bedId}")]
        public async Task<IActionResult> CancelBooking(int bedId)
        {
            var result = await _roomService.CancelBookingAsync(bedId);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPut("update-patient/{patientId}")]
        public async Task<IActionResult> UpdatePatient(
            int patientId,
            [FromBody] UpdatePatientDto dto)
                {
                    var result = await _roomService.UpdatePatientAsync(
                        patientId,
                        dto
                    );

                    if (!result.Success)
                    {
                        return BadRequest(result);
                    }

                    return Ok(result);
                }

    }
}