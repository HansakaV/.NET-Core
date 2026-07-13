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
        public AuthService(IAuthRepository authRepository , IConfiguration configuration)
        {
            _authRepository = authRepository;
            _configuration = configuration;
        }
        public async Task<bool> ForgotPasswordAsync(ForgotPasswordRequestDto forgotPasswordRequest)
        {
            var user = await _authRepository.GetByEmailAsync(forgotPasswordRequest.Email);
            if(user == null) throw new KeyNotFoundException("Email Not Found");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(forgotPasswordRequest.NewPassword);
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
                _configuration.GetSection("AppSettings:TokenSecret").Value ?? throw new InvalidOperationException("Secret Not Found")
                ));
            
            var creds = new SigningCredentials(key , SecurityAlgorithms.HmacSha512Signature);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = creds
            };
             var tokenHandler = new JwtSecurityTokenHandler();
             var token = tokenHandler.CreateToken(tokenDescriptor);

             return tokenHandler.WriteToken(token);
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

    }
}