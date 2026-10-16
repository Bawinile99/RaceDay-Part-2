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
public class EventsController : ControllerBase
{
    private readonly RaceDayDbContext _db;
    public EventsController(RaceDayDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var events = await _db.Events
            .Include(e => e.Organiser)
            .Include(e => e.Enrolments)
            .Select(e => new EventResponseDto(
                e.EventId, e.Name, e.Description, e.EventDate, e.Location,
                e.Distance, e.EventType, e.BannerImageUrl,
                e.OrganiserId,
                e.Organiser.FirstName + " " + e.Organiser.LastName,
                e.Enrolments.Count))
            .ToListAsync();

        return Ok(events);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var e = await _db.Events
            .Include(x => x.Organiser)
            .Include(x => x.Enrolments)
            .FirstOrDefaultAsync(x => x.EventId == id);

        if (e == null) return NotFound();

        return Ok(new EventResponseDto(e.EventId, e.Name, e.Description, e.EventDate,
            e.Location, e.Distance, e.EventType, e.BannerImageUrl,
            e.OrganiserId,
            e.Organiser.FirstName + " " + e.Organiser.LastName,
            e.Enrolments.Count));
    }

    [HttpPost]
    [RequireRole("Organiser")]
    public async Task<IActionResult> Create([FromBody] CreateEventDto dto)
    {
        var organiserId = HttpContext.Session.GetUserId()!.Value;

        var ev = new Event
        {
            Name = dto.Name,
            Description = dto.Description,
            EventDate = dto.EventDate,
            Location = dto.Location,
            Distance = dto.Distance,
            EventType = dto.EventType,
            BannerImageUrl = dto.BannerImageUrl,
            OrganiserId = organiserId
        };

        _db.Events.Add(ev);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = ev.EventId }, new { ev.EventId });
    }

    [HttpPut("{id:int}")]
    [RequireRole("Organiser")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEventDto dto)
    {
        var ev = await _db.Events.FindAsync(id);
        if (ev == null) return NotFound();

        var organiserId = HttpContext.Session.GetUserId()!.Value;
        if (ev.OrganiserId != organiserId) return Forbid();

        ev.Name = dto.Name;
        ev.Description = dto.Description;
        ev.EventDate = dto.EventDate;
        ev.Location = dto.Location;
        ev.Distance = dto.Distance;
        ev.EventType = dto.EventType;
        ev.BannerImageUrl = dto.BannerImageUrl;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [RequireRole("Organiser")]
    public async Task<IActionResult> Delete(int id)
    {
        var ev = await _db.Events.FindAsync(id);
        if (ev == null) return NotFound();

        var organiserId = HttpContext.Session.GetUserId()!.Value;
        if (ev.OrganiserId != organiserId) return Forbid();

        _db.Events.Remove(ev);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
