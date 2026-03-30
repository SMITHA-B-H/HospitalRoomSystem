using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalRoomAPI.Models
{
    public class Setting
    {
        [Key]
        public int Id { get; set; }

        // Relations
        public int HospitalId { get; set; }
        public int? FloorId { get; set; }
        public int? RoomId { get; set; }

        // Settings fields
        public string ScrollingMessage { get; set; } = "";
      
        public int AdsVolume { get; set; } = 50;

        public bool ShowClock { get; set; }

        // Logo path for hospital-level settings
        public string LogoUrl { get; set; } = "";

        public int ScrollingSpeed { get; set; } = 10; // ? NEW

        // Navigation properties (optional)
        [ForeignKey("HospitalId")]
        public Hospital Hospital { get; set; }

        [ForeignKey("FloorId")]
        public Floor Floor { get; set; }

        [ForeignKey("RoomId")]
        public Room Room { get; set; }
    }
}