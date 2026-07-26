
using StudentManagement.API.Models;

namespace StudentManagement.API.Interfaces
{
    public interface IRefreshTokenRepository
    {
        Task AddAsync(RefreshToken refreshToken);
        Task<RefreshToken?> GetHashWithUserAsync(string tokenhash);
        Task SaveChangesAsync();
    }
}