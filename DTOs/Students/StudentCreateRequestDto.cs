using System.ComponentModel.DataAnnotations;

namespace StudentManagement.API.DTOs
{
    public class StudentCreateRequestDto
    {
        [Required]
        public string Name { get; set; } = "";
        [EmailAddress]
        public string Email { get; set; } = "";
        [Required]
        public string Course { get; set; } = "";
        [Required]
        public int Phone { get; set; }
        
    }
}