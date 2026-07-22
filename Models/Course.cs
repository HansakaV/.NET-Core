namespace StudentManagement.API.Models
{
    public class Course
    {
        public int Id {get;set;}
        public string CourseCode {get;set;} = string.Empty;
        public string CourseName {get;set;} = string.Empty;
        public int DurationInMonths {get;set;} 
        public ICollection<Student> Students {get;set;} = new List<Student>();
    }
}