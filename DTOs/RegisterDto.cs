namespace HospitalRoomAPI.DTOs
{
    public class RegisterDto
    {
        public string Name { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public string? LogoUrl { get; set; }

        public string Role { get; set; }  // SuperAdmin or FloorAdmin

        public int? FloorId { get; set; }
    }
}