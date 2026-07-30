using System.ComponentModel.DataAnnotations;
using StudentManagement.API.Models;


namespace StudentManagement.API.DTOs
{
    public class StudentCreateRequestDto
    {
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public int CourseId { get; set; } 
        public string Phone { get; set; } = "";
        
    }
}