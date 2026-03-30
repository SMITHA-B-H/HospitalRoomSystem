using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HospitalRoomAPI.Models
{
    public class Doctor
    {
        public int Id { get; set; }

        [Required]
        public string? Name { get; set; }

        //public int FloorId { get; set; }

        [Required]
        public string? Department { get; set; }

        public string? PhotoUrl { get; set; }

        public int HospitalId { get; set; }

        public Hospital Hospital { get; set; }

    }
}