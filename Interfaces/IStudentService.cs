using StudentManagement.API.DTOs;
using StudentManagement.API.DTOs.Students;

namespace StudentManagement.API.Interfaces
{
    public interface IStudentService
    {
        Task<List<StudentResponseDto>> GetAllAsync(StudentQueryParameters queryParameters);
        Task<StudentResponseDto?> GetByIdAsync(int id);
        Task<StudentResponseDto> CreateAsync(StudentCreateRequestDto student);
        Task UpdateAsync(StudentUpdateRequestDto student);
        Task DeleteAsync(int id);

    }
}