using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VidSync.API.DTOs;
using VidSync.Domain.Entities;

namespace VidSync.API.Controllers;

[ApiController]
[Route("api/[Controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<User> _userManager;

    public AuthController(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserRegisterDto registerDto)
    {
        string username = registerDto.FirstName.Trim() + registerDto.MiddleName?.Trim() + registerDto.LastName?.Trim();

        var userExists = await _userManager.FindByNameAsync(username);
        if (userExists != null)
        {
            ModelState.AddModelError("Username", "Username is already in use.");
            return ValidationProblem(ModelState);
        }

        var emailExists = await _userManager.FindByEmailAsync(registerDto.Email);
        if (emailExists != null)
        {
            ModelState.AddModelError("Email", "Email is already in use.");
            return ValidationProblem(ModelState);
        }

        var user = new User
        {
            UserName = username,
            Email = registerDto.Email,
            SecurityStamp = Guid.NewGuid().ToString(),
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, registerDto.Password);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(error.Code, error.Description);
            }
            return ValidationProblem(ModelState);
        }

        return Ok(new { Message = "User created successfully!" });
    }
}