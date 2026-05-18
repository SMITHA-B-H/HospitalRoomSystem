using Microsoft.AspNetCore.Http;

namespace HospitalRoomAPI.DTOs
{
    public class UploadAnnouncementDto
    {
        public IFormFile File { get; set; }

        public int AnnouncementId { get; set; }
    }
}