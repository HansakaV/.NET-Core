using System.Diagnostics;
using Microsoft.AspNetCore.Routing.Patterns;
using StudentManagement.API.DTOs.Authentication;
using StudentManagement.API.Interfaces;
using StudentManagement.API.Models;

namespace StudentManagement.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepository;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly ITokenService _tokenService;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        public AuthService(IAuthRepository authRepository , IConfiguration configuration, IEmailService emailService, ITokenService tokenService, IRefreshTokenRepository refreshTokenRepository)
        {
            _authRepository = authRepository;
            _configuration = configuration;
            _emailService = emailService;
            _tokenService = tokenService;
            _refreshTokenRepository = refreshTokenRepository;
        }
        public async Task<bool> ForgotPasswordAsync(ForgotPasswordRequestDto forgotPasswordRequest)
        {
            var user = await _authRepository.GetByEmailAsync(forgotPasswordRequest.Email);
            if(user == null) throw new KeyNotFoundException("Email Not Found");

            if(string.IsNullOrEmpty(user.VerificationCode) || user.VerificationCodeExpiry == null) throw new InvalidOperationException
                ("Please Take and enter verification code first");

            if(user.VerificationCodeExpiry < DateTime.UtcNow) throw new InvalidOperationException
                ("Verification Code Expired.Try Again");
            
            if(user.VerificationCode != forgotPasswordRequest.VerificationCode) throw new ArgumentException
                ("verification Code Invalid");
            
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(forgotPasswordRequest.NewPassword);

            user.VerificationCode = null;
            user.VerificationCodeExpiry = null;

            await _authRepository.UpdateUserAsync(user);
            return true;
        }

        public async Task<AuthenticationResultDto> LoginAsync(LoginRequestDto loginRequest,string? ipaddress)
        {
            var user = await _authRepository.GetByEmailAsync(loginRequest.Email);

            if(user == null || !BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Your Email Or Password Incorrect!");
            }

            var accessToken = _tokenService.GenerateAccessToken(user);
            var rawRefreshToken = _tokenService.GenerateRefreshToken();
            var accesTokenExpiration = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["JWTSettings:AccessTokenExpiryMinutes"]!));
            var refreshTokenExpiration = DateTime.UtcNow.AddDays(Convert.ToInt16(_configuration["JWTSettings:RefreshTokenExpiryDays"]!));

            var refreshToken = new RefreshToken()
            {
                TokenHash = _tokenService.HashToken(rawRefreshToken),
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = refreshTokenExpiration,
                CreatedByIp = ipaddress 
            };
            await _refreshTokenRepository.AddAsync(refreshToken);
            await _refreshTokenRepository.SaveChangesAsync();

            return new AuthenticationResultDto
            {
                AccessToken = accessToken,
                RefreshToken = rawRefreshToken,
                AccessTokenExpiresAt = accesTokenExpiration,
                User = new UserResponseDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Role = user.Role
                }
            };
        }

        public async Task LogoutAsync(string rawRefreshToken, string? ipaddres)
        {
            if(rawRefreshToken is null) throw new Exception ("refreshtoken Empty");

            var tokenHash = _tokenService.HashToken(rawRefreshToken);
            var storedToken = await _refreshTokenRepository.GetHashWithUserAsync(tokenHash);
            if(storedToken is null || storedToken.ISRevoked)
            {
                return;
            }
            storedToken.RevokedAt = DateTime.UtcNow;
            storedToken.RevoekdByIp = ipaddres;
            await _refreshTokenRepository.SaveChangesAsync();

        }

        public async Task<AuthenticationResultDto> RefreshAsync(string rawRefreshToken, string? ipaddres)
        {
            if(rawRefreshToken is null) throw new KeyNotFoundException ("refresh Token Empty");

            var tokenHash = _tokenService.HashToken(rawRefreshToken);
            var storedToken = await _refreshTokenRepository.GetHashWithUserAsync(tokenHash);

            if(storedToken is null) throw new UnauthorizedAccessException ("Invalid Refresh Token");
            if(!storedToken.IsActive) throw new UnauthorizedAccessException ("Refresh Token Is Expired or Revoked");

            var newRawRefreshToken = _tokenService.GenerateRefreshToken();
            var newTokenHash = _tokenService.HashToken(newRawRefreshToken);

            storedToken.RevokedAt = DateTime.UtcNow;
            storedToken.RevoekdByIp = ipaddres;
            storedToken.ReplacedByTokenHash = newTokenHash;
            var refreshTokenExpiration = DateTime.UtcNow.AddDays(Convert.ToInt16(_configuration["JWTSettings:RefreshTokenExpiryDays"]!));
            var accesTokenExpiration = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["JWTSettings:AccessTokenExpiryMinutes"]!));

            var newRefreshToken = new RefreshToken
            {
                TokenHash = newTokenHash,
                UserId = storedToken.UserId,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = refreshTokenExpiration,
                CreatedByIp = ipaddres
            };
            await _refreshTokenRepository.AddAsync(newRefreshToken);
            await _refreshTokenRepository.SaveChangesAsync();

            var accessToken = _tokenService.GenerateAccessToken(storedToken.User);

            return new AuthenticationResultDto
            {
                AccessToken = accessToken,
                RefreshToken = newRawRefreshToken,
                AccessTokenExpiresAt = accesTokenExpiration,
                User = new UserResponseDto
                {
                    Id = storedToken.User.Id,
                    Email = storedToken.User.Email,
                    Role = storedToken.User.Role
                }
            };
        }

        public async Task<string> RegisterAsync(RegisterRequestDto registerRequest)
        {
            var exitingUser = await _authRepository.GetByEmailAsync(registerRequest.Email);
            if(exitingUser != null) throw new ArgumentException("this email already registerd");
            
            var user = new User
            {
                Name = registerRequest.Name,
                Email = registerRequest.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerRequest.Password),
                Role = "User"
            };
            await _authRepository.CreateUserAsync(user);

            return "Account Created Successfully";
            
        }

        public async Task<bool> SendVerifactionCodeAsync(SendVerificationCodeDto sendVerificationCode)
        {
            var user = await _authRepository.GetByEmailAsync(sendVerificationCode.Email);
            if(user == null)throw new KeyNotFoundException("This email Not Registerd!");

            var random = new Random();
            string otp = random.Next(100000, 999999).ToString();

            user.VerificationCode = otp;
            user.VerificationCodeExpiry = DateTime.UtcNow.AddMinutes(2);
            await _authRepository.UpdateUserAsync(user);

            string emailBody =$@"<div style='font-family: Arial, sans-serif; padding: 20px; border: 1px solid #eee; border-radius: 5px;'>
            <h2 style='color: #4CAF50;'>Password Reset Request</h2>
            <p>Use this OTP code For Reset Your Password:</p>
            <div style='background: #f9f9f9; padding: 10px; font-size: 24px; font-weight: bold; text-align: center; letter-spacing: 5px; color: #333;'>
                {otp}
            </div>
            <p style='color: #777; font-size: 12px; margin-top: 20px;'>This OTP expires in 2 Minites.</p>
        </div>";
            await _emailService.SendEmailAsync(user.Email, "Student Management System - OTP",emailBody); 

            return true;

        }
    }
}