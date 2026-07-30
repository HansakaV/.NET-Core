namespace StudentManagement.API.Middlewares.Exceptions
{
    public sealed class CourseFullException : Exception
    {
        public CourseFullException(int courseId)
            :base($"No Available Seats Reming For {courseId}"){}
        
    }
}