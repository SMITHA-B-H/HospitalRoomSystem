using System.ComponentModel.DataAnnotations;
namespace HospitalRoomAPI.Models
{
    public class User
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public string PasswordHash { get; set; }

        public string Role { get; set; }  // "SuperAdmin" or "FloorAdmin"

        public int HospitalId { get; set; }

        public Hospital Hospital { get; set; }

        public int? FloorId { get; set; }  // NULL for SuperAdmin

        public Floor? Floor { get; set; }

        public string? ResetToken { get; set; }

        public DateTime? ResetTokenExpiry { get; set; }
    }
}