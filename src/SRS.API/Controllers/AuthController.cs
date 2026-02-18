using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SRS.Application.DTOs;
using SRS.Application.Interfaces;
using SRS.Infrastructure.Persistence;

namespace SRS.API.Controllers;


[ApiController]
[Route("api/auth")]
public class AuthController(
    AppDbContext context,
    IJwtService jwtService,
    IPasswordHasher passwordHasher)
    : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await context.Users
            .FirstOrDefaultAsync(x => x.Username == request.Username);

        if (user == null ||
            !passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            return Unauthorized("Invalid credentials");
        }

        var token = jwtService.GenerateToken(
            user.Id,
            user.Username,
            user.Role.ToString());

        return Ok(new { Token = token });
    }
}
