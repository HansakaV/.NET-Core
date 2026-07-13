using StudentManagement.API.Models;

namespace StudentManagement.API.Interfaces
{
    public interface IAuthRepository
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User> CreateUserAsync(User user);
        Task UpdateUserAsync(User user);
    } 
}