namespace HospitalRoomAPI.DTOs
{

    public class UploadVideoDto
    {
        public IFormFile File { get; set; }
        public int? FloorId { get; set; }
        public int? RoomId { get; set; }
    }
}