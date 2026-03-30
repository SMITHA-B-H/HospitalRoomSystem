using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalRoomAPI.Models
{
    public class AdsVideo
    {
        public int Id { get; set; }

        public string FileName { get; set; }

        public string FilePath { get; set; }

        public int HospitalId { get; set; }

        // Optional (for floor specific ads)
        public int? FloorId { get; set; }

        // Optional (for room specific ads)
        public int? RoomId { get; set; }

        // Navigation properties
        public Hospital Hospital { get; set; }

        // Optional: Link to settings if needed
        public int? SettingId { get; set; }
        public Setting Setting { get; set; }

        public Floor Floor { get; set; }

        public Room Room { get; set; }
    }
}