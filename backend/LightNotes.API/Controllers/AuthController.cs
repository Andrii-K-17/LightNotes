using LightNotes.Application.DTOs.Auth;
using LightNotes.Application.Services.Auth;
using LightNotes.API.Filters;
using Microsoft.AspNetCore.Mvc;

namespace LightNotes.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{
    private readonly IAuthService _authService = authService;

    /// <summary>
    /// Реєстрація нового користувача.
    /// </summary>
    /// <returns>Інформація про зареєстрованого користувача або помилка.</returns>
    [HttpPost("register")]
    [LogOperation("Register a new user")]
    [ValidateRequestBody("registering a user")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        var response = await _authService.RegisterAsync(request);

        if (response == null)
        {
            return Problem(
                title: "User already exists",
                detail: "A user with this email already exists.",
                statusCode: StatusCodes.Status400BadRequest
            );
        }

        return Ok(response);
    }

    /// <summary>
    /// Вхід користувача в систему.
    /// </summary>
    /// <returns>Інформація про користувача або помилка аутентифікації.</returns>
    [HttpPost("login")]
    [LogOperation("User login")]
    [ValidateRequestBody("logging in")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var response = await _authService.LoginAsync(request);

        if (response == null)
        {
            return Problem(
                title: "Unauthorized",
                detail: "Invalid email or password.",
                statusCode: StatusCodes.Status401Unauthorized
            );
        }

        return Ok(response);
    }
}
