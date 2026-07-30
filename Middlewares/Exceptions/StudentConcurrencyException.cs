namespace StudentManagement.API.Middlewares.Exceptions
{
    public sealed class StudentConcurrencyException : Exception
    {
        public StudentConcurrencyException() :base("This student was modified by another user. " +
            "Reload the latest data and try again.")
        {}
        
        public StudentConcurrencyException(string message) :base(message)
        {}
    }
}