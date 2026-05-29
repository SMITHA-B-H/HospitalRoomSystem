namespace HospitalRoomAPI.Models
{
    public class BedDisplayDto
    {
        public int bedNumber { get; set; }
        public string status { get; set; }

        public string patientName { get; set; }
        public string PatientType { get; set; }
        public string doctorName { get; set; }
        public string doctorPhoto { get; set; }
        public string doctorDepartment { get; set; }
    }
}