
using CampusPlacementManagement.DTOs.Auth;
using CampusPlacementManagement.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampusPlacementManagement.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(RegisterRequestDto request)
    {
        var result = await _authService.RegisterAsync(request);
        return Ok(result);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginRequestDto request)
    {
        try
        {
            var result = await _authService.LoginAsync(request);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("student-password-setup-token")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> GenerateStudentPasswordSetupToken(
        StudentPasswordSetupRequestDto request)
    {
        try
        {
            var token = await _authService.GenerateStudentPasswordSetupTokenAsync(
                request.Email);

            return Ok(new
            {
                message = "Password setup token generated.",
                token
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("student-password-setup")]
    [AllowAnonymous]
    public async Task<IActionResult> SetStudentPassword(
        SetStudentPasswordRequestDto request)
    {
        var success = await _authService.SetStudentPasswordAsync(request);

        if (!success)
        {
            return BadRequest(new
            {
                message = "Invalid request or password setup token."
            });
        }

        return Ok(new { message = "Student password set successfully." });
    }
}
