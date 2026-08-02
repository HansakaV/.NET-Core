using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.API.DTOs.Authentication;
using StudentManagement.API.Interfaces;
using StudentManagement.API.util;

namespace StudentManagement.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class LoginController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IConfiguration _configuration;
    private IHostEnvironment _env;
    public LoginController(IAuthService authService, IConfiguration configuration,IHostEnvironment environment)
    {
        _authService = authService;
        _configuration = configuration;
        _env = environment;
    }

[HttpPost("register")]
public async Task<IActionResult> Register(RegisterRequestDto registerRequest)
    {
        await _authService.RegisterAsync(registerRequest);
        return StatusCode(StatusCodes.Status201Created, new{message = "User Registred Successfully"});
    }

[HttpPost("login")]
public async Task<ActionResult<LoginResponseDto>> Login(LoginRequestDto loginRequest)
    {
        var ipaddres = HttpContext.Connection.RemoteIpAddress?.ToString();
        var res = await _authService.LoginAsync(loginRequest,ipaddres);
        var refreshTokenExpiration = DateTime.UtcNow.AddDays(Convert.ToInt16(_configuration["JWTSettings:RefreshTokenExpiryDays"]!));

        this.SetRefreshTokenCookie(res.RefreshToken, refreshTokenExpiration, _env.IsDevelopment());
        return Ok(new LoginResponseDto
        {
            AccessToken = res.AccessToken,
            AccessTokenExpireAt = res.AccessTokenExpiresAt,
            User = res.User
        });
    }

[HttpPost("refresh")]
public async Task<ActionResult<LoginResponseDto>> Refresh()
    {
        var refreshToken = Request.Cookies["RefreshToken"];
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return Unauthorized(new
            { 
                message = "RefreshToken Missing"
            });
        }
        var ipaddres = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _authService.RefreshAsync(refreshToken,ipaddres);
        var refreshTokenExpiration = DateTime.UtcNow.AddDays(Convert.ToInt16(_configuration["JWTSettings:RefreshTokenExpiryDays"]!));

       this.SetRefreshTokenCookie(result.RefreshToken, refreshTokenExpiration, _env.IsDevelopment());

        return Ok(new LoginResponseDto
        {
            AccessToken = result.AccessToken,
            AccessTokenExpireAt = result.AccessTokenExpiresAt,
            User = result.User
        });
    }

[HttpPost("logout")]
public async Task<IActionResult> LogoutAsync()
    {
        var refreshToken = Request.Cookies["RefreshToken"];
        if (!string.IsNullOrWhiteSpace(refreshToken)){
            var ipaddres = HttpContext.Connection
                .RemoteIpAddress?
                .ToString();
            await _authService.LogoutAsync(refreshToken,ipaddres);
        }
        Response.Cookies.Delete("RefreshToken", new CookieOptions
        {
            HttpOnly = true,
            Secure = !_env.IsDevelopment(),
            SameSite = SameSiteMode.Strict,
            Path = "/api/login"
        });
        return NoContent();
    }

[HttpPost("forgot-password")]
public async Task<IActionResult> ForgotPassword(ForgotPasswordRequestDto forgotPasswordRequest)
    {
        await _authService.ForgotPasswordAsync(forgotPasswordRequest);
        return StatusCode(StatusCodes.Status200OK);
    }

[HttpPost("send-verification-code")]
public async Task<IActionResult> SendOTP([FromBody] SendVerificationCodeDto verificationCodeDto)
    {
        await _authService.SendVerifactionCodeAsync(verificationCodeDto);
        return StatusCode(StatusCodes.Status200OK);
    }       
}

    
