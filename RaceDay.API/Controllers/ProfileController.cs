using Microsoft.AspNetCore.Mvc;
using RaceDay.API.Attributes;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Services;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[RequireRole]
public class ProfileController : ControllerBase
{
    private readonly RaceDayDbContext _db;
    public ProfileController(RaceDayDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var id = HttpContext.Session.GetUserId()!.Value;
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();

        return Ok(new UserResponseDto(user.UserId, user.FirstName, user.LastName,
            user.Email, user.Role, user.PhoneNumber, user.DateOfBirth, user.ProfilePictureUrl));
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateProfileDto dto)
    {
        var id = HttpContext.Session.GetUserId()!.Value;
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();

        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.PhoneNumber = dto.PhoneNumber;
        user.DateOfBirth = dto.DateOfBirth;
        user.ProfilePictureUrl = dto.ProfilePictureUrl;

        await _db.SaveChangesAsync();
        return NoContent();
    }
}
