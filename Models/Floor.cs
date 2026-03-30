using System.ComponentModel.DataAnnotations;
namespace HospitalRoomAPI.Models
{
    public class Floor
    {
        public int Id { get; set; }

        public string FloorName { get; set; }

        public int HospitalId { get; set; }

        public Hospital Hospital { get; set; }

        public List<Room>? Rooms { get; set; }

        public ICollection<User>? Users { get; set; }
    }
}