using StudentManagement.API.Enums;

namespace StudentManagement.API.DTOs
{
    public class StudentUpdateRequestDto
    {
        public int Id { get; set; }
        public string? Name { get; set; } = null;
        public Courses? Course { get; set; } = null;
        public string? Email { get; set; } = null;
        public string? Phone { get; set; } = null;
    }
}