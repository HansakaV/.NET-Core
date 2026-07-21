using StudentManagement.API.Models;

namespace StudentManagement.API.Interfaces
{
    public interface IStudentRepository
    {
        Task<List<Student>> GetAllAsync();
        Task<Student?> GetByIdAsync(int id);
        Task<Student?> GetByEmailAsync(string email);
        Task<Student> CreateAsync(Student student);
        Task UpdateAsync(Student student);
        Task<int> DeleteAsync(int id);
    }
}