using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HospitalRoomAPI.Models
{
    public class Bed
    {
    
        public int Id { get; set; }

        [Required]
        public string? Status { get; set; } = "Available";

        [Required]
        public int BedNumber { get; set; }

        public int RoomId { get; set; }

        // ? Add this
        public int? PatientId { get; set; }
        public Patient? Patient { get; set; }

        [JsonIgnore]
        public Room? Room { get; set; }
    }
}