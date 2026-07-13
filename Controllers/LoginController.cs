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
        return StatusCode(StatusCodes.Status201Created);
    }
}
    
        
    
