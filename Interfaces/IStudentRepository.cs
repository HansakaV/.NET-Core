using StudentManagement.API.DTOs;
using StudentManagement.API.DTOs.Students;
using StudentManagement.API.Models;
using StudentManagement.API.util;

namespace StudentManagement.API.Interfaces
{
    public interface IStudentRepository
    {
        Task<PagedResult<StudentResponseDto>> GetAllAsync(StudentQueryParameters queryParameters);
        Task<Student?> GetByIdAsync(int id);
        Task<Student?> GetByEmailAsync(string email);
        Task<Student> CreateAsync(Student student);
        Task UpdateAsync(Student student);
        Task<int> DeleteAsync(int id);
    }
}