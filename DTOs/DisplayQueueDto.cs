namespace HospitalRoomAPI.DTOs
{
    public class DisplayQueueDto
    {
        public string DisplayNumber { get; set; } = "";

        public DoctorDisplayDto? Doctor { get; set; }

        public List<QueueItemDto> Queue { get; set; }
            = new();
    }

   
}