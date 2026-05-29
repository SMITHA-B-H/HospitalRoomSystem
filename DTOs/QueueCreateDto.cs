namespace HospitalRoomAPI.DTOs
{
    public class QueueCreateDto
    {
        public string PatientName { get; set; }
        public string Department { get; set; }
        public int DoctorId { get; set; }
        public string Role { get; set; } = string.Empty;
        //public string Status { get; set; } = "Waiting";
        //public string Stage { get; set; } = "Initial";  
        //public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string DisplayNumber { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = "";
    }
}