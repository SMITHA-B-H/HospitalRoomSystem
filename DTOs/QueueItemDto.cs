namespace HospitalRoomAPI.DTOs
{
    public class QueueItemDto
    {
        public int Id { get; set; }

        public int TokenNumber { get; set; }

        public string PatientName { get; set; } = "";

        public string Status { get; set; } = "";

        public string Stage { get; set; } = "";
    }
}