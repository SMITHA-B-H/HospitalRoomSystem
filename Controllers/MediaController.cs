using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;
namespace HospitalRoomAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MediaController : ControllerBase
    {
        [HttpGet("stream-video")]
        public async Task<IActionResult> StreamVideo(string fileId)
        {
            if (string.IsNullOrEmpty(fileId))
                return BadRequest("FileId required");

            var url = $"https://drive.google.com/uc?export=download&id={fileId}";

            using var client = new HttpClient();

            var response = await client.SendAsync(
                new HttpRequestMessage(HttpMethod.Get, url),
                HttpCompletionOption.ResponseHeadersRead
            );

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode);

            var stream = await response.Content.ReadAsStreamAsync();

            var contentType = response.Content.Headers.ContentType?.ToString() ?? "video/mp4";

            return File(stream, contentType, enableRangeProcessing: true);
        }
    }
}