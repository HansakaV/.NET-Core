using StudentManagement.API.DTOs;

namespace StudentManagement.API.Interfaces
{
    public interface IStudentService
    {
        Task<List<StudentResponseDto>> GetAllAsync();
        Task<StudentResponseDto?> GetByIdAsync(int id);
        Task<StudentResponseDto> CreateAsync(StudentCreateRequestDto student);
        Task UpdateAsync(StudentUpdateRequestDto student);
        Task DeleteAsync(int id);

    }
}