namespace StudentManagement.API.DTOs.Enrollment
{
    public class EnrollmentResponseDTO
    {
        public int Id {get;set;}
        public int StudentId {get;set;}
        public string StudentName = string.Empty;
        public int CourseId {get;set;}
        public string CourseName {get;set;} = string.Empty;
        public DateTime EnrolledAt {get;set;}
    }
}