namespace StudentManagement.API.Middlewares.Exceptions
{
    public class DuplicateEnrollmentException : Exception
    {
        public DuplicateEnrollmentException(int studentId, int courseId) 
                :base($"Student {studentId} already enrolled with this Cousrse {courseId}."){}
        
    }
}