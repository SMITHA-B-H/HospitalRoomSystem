namespace HospitalRoomAPI.DTOs
{
    public class AnnouncementDto
    {
        public string PatientName { get; set; }

        public string Message { get; set; }

        public int? RoomId { get; set; }

        public int? FloorId { get; set; }

        public string? PosterUrl { get; set; }

        public string? VideoUrl { get; set; }

        public int HospitalId { get; set; }

        public int ExpiryHours { get; set; }
    }
}