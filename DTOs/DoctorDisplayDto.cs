namespace HospitalRoomAPI.DTOs
{
    public class DoctorDisplayDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = "";

        public string Department { get; set; } = "";

        public string Role { get; set; } = "";

        public string PhotoUrl { get; set; } = "";

        public string DisplayNumber { get; set; } = "";
    }
}