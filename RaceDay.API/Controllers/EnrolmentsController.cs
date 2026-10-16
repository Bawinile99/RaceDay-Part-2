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
public class EnrolmentsController : ControllerBase
{
    private readonly RaceDayDbContext _db;
    public EnrolmentsController(RaceDayDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetMine()
    {
        var userId = HttpContext.Session.GetUserId()!.Value;
        var role = HttpContext.Session.GetRole();

        IQueryable<Enrolment> query = _db.Enrolments
            .Include(e => e.Participant)
            .Include(e => e.Event)
            .Include(e => e.Category);

        query = role == "Participant"
            ? query.Where(e => e.ParticipantId == userId)
            : query.Where(e => e.Event.OrganiserId == userId);

        var list = await query.Select(e => new EnrolmentResponseDto(
            e.EnrolmentId, e.ParticipantId,
            e.Participant.FirstName + " " + e.Participant.LastName,
            e.EventId, e.Event.Name,
            e.CategoryId, e.Category.Name,
            e.EnrolmentDate, e.Status)).ToListAsync();

        return Ok(list);
    }

    [HttpPost]
    [RequireRole("Participant")]
    public async Task<IActionResult> Create([FromBody] CreateEnrolmentDto dto)
    {
        var userId = HttpContext.Session.GetUserId()!.Value;

        var ev = await _db.Events.FindAsync(dto.EventId);
        if (ev == null) return NotFound(new { message = "Event not found." });

        var cat = await _db.Categories
            .FirstOrDefaultAsync(c => c.CategoryId == dto.CategoryId && c.EventId == dto.EventId);
        if (cat == null)
            return BadRequest(new { message = "Category does not belong to this event." });

        if (await _db.Enrolments.AnyAsync(e => e.ParticipantId == userId && e.EventId == dto.EventId))
            return Conflict(new { message = "Already enrolled in this event." });

        var enrolment = new Enrolment
        {
            ParticipantId = userId,
            EventId = dto.EventId,
            CategoryId = dto.CategoryId,
            Status = EnrolmentStatus.Confirmed
        };

        _db.Enrolments.Add(enrolment);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetMine), new { id = enrolment.EnrolmentId },
            new { enrolment.EnrolmentId });
    }

    [HttpDelete("{id:int}")]
    [RequireRole("Participant")]
    public async Task<IActionResult> Cancel(int id)
    {
        var userId = HttpContext.Session.GetUserId()!.Value;
        var en = await _db.Enrolments.FindAsync(id);
        if (en == null) return NotFound();
        if (en.ParticipantId != userId) return Forbid();

        _db.Enrolments.Remove(en);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
