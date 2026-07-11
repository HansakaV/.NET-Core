namespace StudentManagement.API.DTOs
{
    public class StudentUpdateRequestDto
    {
        public int Id { get; set; }
        public string? Name { get; set; } = null;
        public string? Course { get; set; } = null;
        public string? Email { get; set; } = null;
        public int? Phone { get; set; }
    }
}