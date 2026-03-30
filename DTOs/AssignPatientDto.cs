namespace HospitalRoomAPI.DTOs
{
    public class AssignPatientDto
    {
        public int BedId { get; set; }

        public string Name { get; set; }
        public int Age { get; set; }

        public int DoctorId { get; set; }
    }
}
