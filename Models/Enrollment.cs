namespace StudentManagement.API.Models
{
    public class Enrollment
    {
        public int Id {get;set;}
        public int StuedntId {get;set;}
        public Student Student {get;set;} = null!;
        public int CourseId {get;set;}
        public Course Course {get;set;} = null!;
        public DateTime EnrolledAt {get;set;} = DateTime.UtcNow;

    }
}