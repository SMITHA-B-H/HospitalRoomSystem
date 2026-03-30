using System.ComponentModel.DataAnnotations;
namespace HospitalRoomAPI.Models
{
    public class Room
    {
        public int Id { get; set; }

        [Required]
        public string? RoomNumber { get; set; }

        [Required]
        public string? RoomName { get; set; }

        [Required]
        public string? RoomType { get; set; }

        [Required]
        public int TotalBeds { get; set; }

        public int FloorId { get; set; }

        public Floor? Floor { get; set; }

        
        public List<Bed>? Beds { get; set; } = new();
    }
}
