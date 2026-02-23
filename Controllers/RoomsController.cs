using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class RoomsController : ControllerBase
{
    [HttpGet]
    public IActionResult GetRooms()
    {
        var rooms = new[]
        {
            new { Id = 1, RoomNumber="101", Status="Available"}
        };

        return Ok(rooms);
    }
}