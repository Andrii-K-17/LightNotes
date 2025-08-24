using LightNotes.Application.Services.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LightNotes.API.Filters;

namespace LightNotes.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController(IUserService userService) : ControllerBase
{
    private readonly IUserService _userService = userService;

    /// <summary>
    /// Deletes a user account by its unique identifier.
    /// </summary>
    [HttpDelete("{userId}")]
    [LogOperation("Delete user by ID")]
    public async Task<IActionResult> DeleteUserAccount(Guid userId)
    {
        var result = await _userService.DeleteUserAccountAsync(userId);

        if (!result)
        {
            return Problem(
                title: "User not found",
                detail: $"User with ID: {userId} was not found.",
                statusCode: StatusCodes.Status404NotFound
            );
        }

        return NoContent();
    }
}
