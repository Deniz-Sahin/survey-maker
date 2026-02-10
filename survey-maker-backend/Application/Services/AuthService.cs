using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SurveyMaker.Infrastructure.Identity;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SurveyMaker.Api.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _config;
        private readonly ILogger<AuthService> _logger;

        public AuthService(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, IConfiguration config, ILogger<AuthService> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _config = config;
            _logger = logger;
        }

        public async Task<string?> GenerateJwtAsync(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return null;

            var result = await _signInManager.CheckPasswordSignInAsync(user, password, false);
            if (!result.Succeeded) return null;

            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty)
            };

            // include role claims so the frontend can read the role from the token
            foreach (var r in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, r));
            }
            
            var jwtKey = _config["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key not configured");

            // DEV ONLY: log the JWT key for debugging. REMOVE in production - logging secrets is insecure.
            _logger.LogDebug("Jwt Key (DEV ONLY): {JwtKey}", jwtKey);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(_config["Jwt:ExpiresMinutes"] ?? "120")),
                signingCredentials: creds
            );

            var handler = new JwtSecurityTokenHandler();
            return handler.WriteToken(token);
        }
    }
}