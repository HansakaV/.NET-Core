using System.ComponentModel.DataAnnotations;
using StudentManagement.API.Enums;

namespace StudentManagement.API.DTOs
{
    public class StudentCreateRequestDto
    {
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public Courses Course { get; set; }
        public string Phone { get; set; } = "";
        
    }
}