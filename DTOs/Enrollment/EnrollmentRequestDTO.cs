namespace StudentManagement.API.DTOs.Enrollment
{
    public class EnrollmentRequestDTO
    {
        public string IdempotencyKey {get;set;} = string.Empty;
        public int StudentId {get;set;}
        public int CourseId {get;set;} 
    }
}