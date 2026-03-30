using Microsoft.AspNetCore.Http;

namespace HospitalRoomAPI.DTOs
{
    public class DoctorDto
    {
        public string Name { get; set; }
        public string Department { get; set; }
        public IFormFile? Photo { get; set; }  // File upload
    }
}