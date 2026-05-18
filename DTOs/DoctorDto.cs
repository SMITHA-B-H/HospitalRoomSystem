using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace HospitalRoomAPI.DTOs
{
    public class DoctorDto
    {
        [Required]
        public string? EmployeeId { get; set; }

        [Required]
        public string? Name { get; set; }

        [Required]
        public string? Department { get; set; }

        [Required]
        public string? Role { get; set; }

        public IFormFile? Photo { get; set; }
    }
}