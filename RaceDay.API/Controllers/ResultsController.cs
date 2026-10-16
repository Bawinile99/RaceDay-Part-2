using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Attributes;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using RaceDay.API.Services;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[RequireRole]
public class ResultsController : ControllerBase
{
    private readonly RaceDayDbContext _db;
    public ResultsController(RaceDayDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetMine()
    {
        var userId = HttpContext.Session.GetUserId()!.Value;
        var role = HttpContext.Session.GetRole();

        var query = _db.Results
            .Include(r => r.Enrolment).ThenInclude(e => e.Participant)
            .Include(r => r.Enrolment).ThenInclude(e => e.Event)
            .Include(r => r.Enrolment).ThenInclude(e => e.Category)
            .AsQueryable();

        query = role == "Participant"
            ? query.Where(r => r.Enrolment.ParticipantId == userId)
            : query.Where(r => r.Enrolment.Event.OrganiserId == userId);

        var list = await query.Select(r => new ResultResponseDto(
            r.ResultId, r.EnrolmentId,
            r.Enrolment.Participant.FirstName + " " + r.Enrolment.Participant.LastName,
            r.Enrolment.Event.Name,
            r.Enrolment.Category.Name,
            r.FinishTimeSeconds, r.FinishingPosition,
            r.CapturedDate,
            TimeSpan.FromSeconds(r.FinishTimeSeconds).ToString(@"hh\:mm\:ss")))
            .ToListAsync();

        return Ok(list);
    }

    [HttpPost]
    [RequireRole("Organiser")]
    public async Task<IActionResult> Create([FromBody] CreateResultDto dto)
    {
        var userId = HttpContext.Session.GetUserId()!.Value;

        var en = await _db.Enrolments
            .Include(e => e.Event)
            .FirstOrDefaultAsync(e => e.EnrolmentId == dto.EnrolmentId);
        if (en == null) return NotFound(new { message = "Enrolment not found." });
        if (en.Event.OrganiserId != userId) return Forbid();

        if (await _db.Results.AnyAsync(r => r.EnrolmentId == dto.EnrolmentId))
            return Conflict(new { message = "Result already captured for this enrolment." });

        var result = new Result
        {
            EnrolmentId = dto.EnrolmentId,
            FinishTimeSeconds = dto.FinishTimeSeconds,
            FinishingPosition = dto.FinishingPosition
        };

        _db.Results.Add(result);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetMine), new { id = result.ResultId },
            new { result.ResultId });
    }
}
