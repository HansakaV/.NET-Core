using Microsoft.EntityFrameworkCore;
using StudentManagement.API.Data;
using StudentManagement.API.Interfaces;
using StudentManagement.API.Models;

namespace StudentManagement.API.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly AppDBContext _Context;
        public RefreshTokenRepository(AppDBContext appDBContext)
        {
            _Context = appDBContext;
        }
        public async Task AddAsync(RefreshToken refreshToken)
        {
            await _Context.RefreshTokens.AddAsync(refreshToken);
        }

        public async Task<RefreshToken?> GetHashWithUserAsync(string tokenhash)
        {
            return await _Context.RefreshTokens
                .Include(token => token.User)
                .FirstOrDefaultAsync(token => token.TokenHash == tokenhash);
        }

        public async Task SaveChangesAsync()
        {
            await _Context.SaveChangesAsync();
        }
    }
}