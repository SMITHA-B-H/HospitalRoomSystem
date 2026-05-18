namespace HospitalRoomAPI.DTOs
{

    public class CentralDisplayDto
    {
        public int DoctorId { get; set; }

        public string DoctorName { get; set; } = "";

        public string DoctorDepartment { get; set; } = "";

        public string RoomNumber { get; set; } = "";

        public int CurrentToken { get; set; }

        public string PatientName { get; set; } = "";

        public string Status { get; set; } = "";
    }
}