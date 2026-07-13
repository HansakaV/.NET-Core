using Microsoft.EntityFrameworkCore;
using StudentManagement.API.Data;
using StudentManagement.API.Interfaces;
using StudentManagement.API.Models;

namespace StudentManagement.API.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly AppDBContext _context;
        public AuthRepository(AppDBContext context) => _context = context;

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Set<User>().FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User> CreateUserAsync(User user)
        {
            _context.Set<User>().Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task UpdateUserAsync(User user)
        {
            _context.Set<User>().Update(user);
            await _context.SaveChangesAsync();
        }
        
        
    }
}