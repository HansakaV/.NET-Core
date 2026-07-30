namespace StudentManagement.API.Middlewares.Exceptions
{
    public sealed class StudentNotFoundException : Exception
    {
        public StudentNotFoundException(int studentId) :base($"Student with ID {studentId} was not found.")
        {}
    }
}