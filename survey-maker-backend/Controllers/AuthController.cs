using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SurveyMaker.Infrastructure.Identity;
using SurveyMaker.Api.Application.Services;
using SurveyMaker.Api.Application.Dtos;

namespace SurveyMaker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _config;
    private readonly IAuthService _auth;

    public AuthController(UserManager<ApplicationUser> userManager, IConfiguration config, IAuthService auth)
    {
        _userManager = userManager;
        _config = config;
        _auth = auth;
    }

    public record RegisterDto(string Email, string Password, string Role);
    public record LoginDto(string Email, string Password);
    public record AuthResponse(string Token, DateTime Expires);

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        var exists = await _userManager.FindByEmailAsync(dto.Email);
        if (exists != null) return BadRequest("Email already in use.");

        var user = new ApplicationUser { UserName = dto.Email, Email = dto.Email, EmailConfirmed = true };
        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded) return BadRequest(result.Errors);

        var role = string.IsNullOrWhiteSpace(dto.Role) ? "User" : dto.Role;
        await _userManager.AddToRoleAsync(user, role);

        return CreatedAtAction(null, new { user.Id, user.Email });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto req)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var token = await _auth.GenerateJwtAsync(req.Email, req.Password);
        if (token == null) return Unauthorized();
        return Ok(new { token });
    }
}