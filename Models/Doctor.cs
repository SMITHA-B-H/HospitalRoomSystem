using System.ComponentModel.DataAnnotations;

namespace HospitalRoomAPI.Models
{
    public class Doctor
    {
        public int Id { get; set; }

        [Required]
        public string? EmployeeId { get; set; }   // EMP001

        [Required]
        public string? Name { get; set; }

        [Required]
        public string? Department { get; set; }

        [Required]
        public string? Role { get; set; }   // Doctor / Nurse / LAB / SCAN / XRAY

        public string? PhotoUrl { get; set; }

        public int HospitalId { get; set; }

        public Hospital Hospital { get; set; }

        public string DisplayNumber { get; set; } = "";
    }
}