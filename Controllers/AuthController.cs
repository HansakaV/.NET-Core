using Microsoft.AspNetCore.Mvc;
using StudentManagement.API.DTOs.Authentication;
using StudentManagement.API.Interfaces;

namespace StudentManagement.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class LoginController : ControllerBase
{
    private readonly IAuthService _authService;
    public LoginController(IAuthService authService)
    {
        _authService = authService;
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
        var res = await _authService.LoginAsync(loginRequest);
        return Ok(res);
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

    
