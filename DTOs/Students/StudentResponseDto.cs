using StudentManagement.API.Enums;

namespace StudentManagement.API.DTOs
{
    public class StudentResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public Courses Course ;
        public string Phone { get; set; } = "";
    }
}