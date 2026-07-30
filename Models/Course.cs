namespace StudentManagement.API.Models
{
    public class Course
    {
        public int Id {get;set;}
        public string CourseCode {get;set;} = string.Empty;
        public string CourseName {get;set;} = string.Empty;
        public int DurationInMonths {get;set;} 
        public int AvailableSeats {get;set;}
        public int Version {get;set;} = 1;
        public ICollection<Student> Students {get;set;} = new List<Student>();
    }
}