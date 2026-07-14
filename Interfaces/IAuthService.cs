using StudentManagement.API.DTOs.Authentication;

namespace StudentManagement.API.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponseDto> LoginAsync(LoginRequestDto loginRequest);
        Task<string> RegisterAsync(RegisterRequestDto registerRequest);
        Task<bool> ForgotPasswordAsync(ForgotPasswordRequestDto forgotPasswordRequest);
        Task<bool> SendVerifactionCodeAsync(SendVerificationCodeDto sendVerificationCode);
    }
}