namespace HospitalRoomAPI.DTOs
{
    public class UpdateFloorDto
    {
        public string FloorName { get; set; }

        public string AdminName { get; set; }

        public string AdminEmail { get; set; }

        public string? Password { get; set; }
    }
}