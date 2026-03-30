namespace HospitalRoomAPI.DTOs
{
    public class RegisterFloorAdminDto
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public int FloorId { get; set; }
    }
}