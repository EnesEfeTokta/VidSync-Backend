using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VidSync.API.DTOs;
using VidSync.Domain.Entities;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace VidSync.API.Controllers;

[ApiController]
[Route("api/[Controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly IConfiguration _configuration;

    public AuthController(UserManager<User> userManager, IConfiguration configuration)
    {
        _userManager = userManager;
        _configuration = configuration;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserLoginDto loginDto)
    {
        var user = await _userManager.FindByEmailAsync(loginDto.Email);
        if (user == null)
        {
            ModelState.AddModelError("Email", "Email not found.");
            return ValidationProblem(ModelState);
        }

        var result = await _userManager.CheckPasswordAsync(user, loginDto.Password);
        if (!result)
        {
            ModelState.AddModelError("Password", "Invalid password.");
            return ValidationProblem(ModelState);
        }

        var token = GenerateJwtToken(user);

        return Ok(new
        {
            Token = token,
            User = new
            {
                user.Id,
                user.FirstName,
                user.LastName,
                user.Email,
                user.UserName
            }
        });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserRegisterDto registerDto)
    {
        string username = GenerateUsername(registerDto.FirstName, registerDto.MiddleName, registerDto.LastName);

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
            FirstName = registerDto.FirstName ?? "N/A",
            MiddleName = registerDto.MiddleName ?? "N/A",
            LastName = registerDto.LastName ?? "N/A",
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

        var token = GenerateJwtToken(user);

        return Ok(new
        {
            Token = token,
            User = new
            {
                user.Id,
                user.FirstName,
                user.LastName,
                user.Email,
                user.UserName
            }
        });
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString))
        {
            ModelState.AddModelError("Token", "Token is missing the NameIdentifier claim.");
            return Unauthorized(ModelState);
        }

        if (!Guid.TryParse(userIdString, out Guid userIdGuid))
        {
            ModelState.AddModelError("Token", "Invalid user identifier format in token.");
            return BadRequest(ModelState);
        }

        var user = await _userManager.FindByIdAsync(userIdString);
        if (user == null)
        {
            ModelState.AddModelError("Token", "User not found.");
            return NotFound(ModelState);
        }

        var userDto = new
        {
            user.Id,
            user.UserName,
            user.Email,
            user.FirstName,
            user.MiddleName,
            user.LastName,
            user.CreatedAt
        };

        return Ok(userDto);
    }

    private string GenerateUsername(string firstName, string? middleName, string? lastName)
    {
        return firstName.Trim() + middleName?.Trim() + lastName?.Trim();
    }

    private string GenerateJwtToken(User user)
    {
        var Claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserName ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
            new Claim(ClaimTypes.Role, "User")
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? string.Empty));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? string.Empty,
            audience: _configuration["Jwt:Audience"] ?? string.Empty,
            claims: Claims,
            expires: DateTime.Now.AddMinutes(30),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}