using StudentManagement.API.DTOs.Authentication;
using StudentManagement.API.Interfaces;
using StudentManagement.API.Models;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace StudentManagement.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepository;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        public AuthService(IAuthRepository authRepository , IConfiguration configuration, IEmailService emailService)
        {
            _authRepository = authRepository;
            _configuration = configuration;
            _emailService = emailService;
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
            user.VerificationCode = null;

            await _authRepository.UpdateUserAsync(user);
            return true;
        }

        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto loginRequest)
        {
            var user = await _authRepository.GetByEmailAsync(loginRequest.Email);

            if(user == null || !BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Your Email Or Password Incorrect!");
            }

            return new LoginResponseDto{Token = CreateJwtToken(user)};
        }

        private string CreateJwtToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration["JwtSettings:TokenSecret"]!));
            
            var creds = new SigningCredentials(key , SecurityAlgorithms.HmacSha512Signature);
            var token = new JwtSecurityToken(
                issuer:_configuration["JwtSettings:Issuer"],
                audience:_configuration["JwtSettings:Audience"],
                claims : claims,
                expires : DateTime.UtcNow.AddMinutes(
                    Convert.ToDouble(
                        _configuration["JwtSettings:ExpiryMinutes"])),
                signingCredentials: creds
                 
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
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