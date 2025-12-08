using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TegWallet.Application.Features.Core.Clients.Dto;
using TegWallet.Application.Features.Core.Clients.Query;
using TegWallet.Application.Helpers;
using TegWallet.Domain.Entity.Auth;

namespace TegWallet.WalletApi.Controllers.Auth;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}")]
public class AuthController(UserManager<ApplicationUser> userManager, IConfiguration config) : ApiControllerBase<AuthController>
{
    [MapToApiVersion("1.0")]
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody]LoginDto dto)
    {
        var user = await userManager.FindByEmailAsync(dto.Email);
        if (user == null)
            return Unauthorized("Invalid credentials");

        var valid = await userManager.CheckPasswordAsync(user, dto.Password);
        if (!valid)
            return Unauthorized("Invalid credentials");

        var (token, expiresInSeconds) = GenerateJwtToken(user);

        return Ok(new
        {
            access_token = token,
            token_type = "Bearer",
            expires_in = expiresInSeconds
        });
    }

    [HttpGet("profile")]
    public async Task<ActionResult<Result<UserProfileDto>>> GetProfile()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var command = new GetUserProfileQuery(userId);

        var result = await MediatorSender.Send(command);

        return Ok(result);
    }

    private (string token, int expiresInSeconds) GenerateJwtToken(ApplicationUser user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["JwtSettings:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new Claim("fullname", $"{user.FirstName} {user.LastName}")
        };

        // Set expiration duration
        var expires = DateTime.UtcNow.AddHours(1); // 1 hour expiry
        var expiresInSeconds = (int)TimeSpan.FromHours(1).TotalSeconds;

        var token = new JwtSecurityToken(
            issuer: config["JwtSettings:Issuer"],
            audience: config["JwtSettings:Audience"], // optional
            claims: claims,
            expires: expires,
            signingCredentials: creds);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return (tokenString, expiresInSeconds);
    }

}

public record RegisterDto(string Email, string PhoneNumber, string FirstName, string LastName, string Password);
public record LoginDto(string Email, string Password);
public record LoginResponse(string Token, DateTime Password);