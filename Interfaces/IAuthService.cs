using StudentManagement.API.DTOs.Authentication;

namespace StudentManagement.API.Interfaces
{
    public interface IAuthService
    {
        Task<AuthenticationResultDto> LoginAsync(LoginRequestDto loginRequest, string? ipaddres);
        Task<string> RegisterAsync(RegisterRequestDto registerRequest);
        Task<bool> ForgotPasswordAsync(ForgotPasswordRequestDto forgotPasswordRequest);
        Task<bool> SendVerifactionCodeAsync(SendVerificationCodeDto sendVerificationCode);
        Task<AuthenticationResultDto> RefreshAsync(string rawRefreshToken , string? ipaddres);
        Task LogoutAsync(string rawRefreshToken, string? ipaddres);
    }
}