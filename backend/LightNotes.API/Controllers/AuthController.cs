using LightNotes.Application.DTOs.Auth;
using LightNotes.Application.Services.Auth;
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
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var response = await _authService.RegisterAsync(request);

        if (response == null)
        {
            return BadRequest(new { message = "Користувач з таким email вже існує." });
        }

        return Ok(response);
    }

    /// <summary>
    /// Вхід користувача в систему.
    /// </summary>
    /// <returns>Інформація про користувача або помилка автентифікації.</returns>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var response = await _authService.LoginAsync(request);

        if (response == null)
        {
            return Unauthorized(new { message = "Невірний email або пароль." });
        }

        return Ok(response);
    }
}
