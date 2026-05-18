namespace HospitalRoomAPI.DTOs
{ 

    public class AnnouncementDisplayDto
    {
        public int Id { get; set; }
        public string? PatientName { get; set; }
        public string? Message { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiryTime { get; set; }
        public string? PosterUrl { get; set; }
        public string? VideoUrl { get; set; }
        public bool IsActive { get; set; }
        public int? ExpiryHours { get; set; }
    }
}