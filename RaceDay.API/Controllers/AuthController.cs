using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using RaceDay.API.Services;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly RaceDayDbContext _db;
    private readonly IRaceDayPasswordHasher _hasher;

    public AuthController(RaceDayDbContext db, IRaceDayPasswordHasher hasher)
    {
        _db = db;
        _hasher = hasher;
    }

    /// <summary>Registers a new user (Organiser or Participant).</summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (await _db.Users.AnyAsync(u => u.Email == dto.Email))
            return Conflict(new { message = "Email already registered." });

        var user = new User
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            PasswordHash = _hasher.Hash(dto.Password),
            Role = dto.Role,
            PhoneNumber = dto.PhoneNumber,
            DateOfBirth = dto.DateOfBirth
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var response = new UserResponseDto(user.UserId, user.FirstName, user.LastName,
            user.Email, user.Role, user.PhoneNumber, user.DateOfBirth, user.ProfilePictureUrl);

        return CreatedAtAction(nameof(Register), new { id = user.UserId }, response);
    }

    /// <summary>Logs in a user and starts a session.</summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user == null || !_hasher.Verify(dto.Password, user.PasswordHash))
            return Unauthorized(new { message = "Invalid credentials." });

        HttpContext.Session.SetUser(user.UserId, user.Role.ToString());

        return Ok(new UserResponseDto(user.UserId, user.FirstName, user.LastName,
            user.Email, user.Role, user.PhoneNumber, user.DateOfBirth, user.ProfilePictureUrl));
    }

    /// <summary>Logs out the current user.</summary>
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return NoContent();
    }
}
