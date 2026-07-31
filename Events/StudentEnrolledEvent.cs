
namespace StudentManagement.API.Events
{
    public record StudentEnrolledEvent
    {
        public int StuedntId {get;set;}
        public int CourseId {get;set;}
        public DateTime EnrolledAt {get;set;}
    }
}