using Microsoft.AspNetCore.Mvc;

namespace HospitalRoomAPI.Controllers
{
    [ApiController]
    [Route("videos")]
    public class VideosController : ControllerBase
    {
        private readonly string _videoRoot;

        public VideosController()
        {
            _videoRoot = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "videos"
            );
        }

        [HttpGet("{fileName}")]
        public IActionResult GetVideo(string fileName)
        {
            var filePath = Path.Combine(_videoRoot, fileName);

            if (!System.IO.File.Exists(filePath))
                return NotFound("Video not found");

            var stream = new FileStream(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read
            );

            return File(stream, "video/mp4", enableRangeProcessing: true);
        }
    }
}