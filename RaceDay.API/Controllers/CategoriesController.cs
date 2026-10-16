using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Attributes;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using RaceDay.API.Services;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/events/{eventId:int}/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly RaceDayDbContext _db;
    public CategoriesController(RaceDayDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll(int eventId)
    {
        if (!await _db.Events.AnyAsync(e => e.EventId == eventId)) return NotFound();

        var cats = await _db.Categories
            .Where(c => c.EventId == eventId)
            .Select(c => new CategoryResponseDto(c.CategoryId, c.Name, c.MinAge, c.MaxAge, c.EventId))
            .ToListAsync();

        return Ok(cats);
    }

    [HttpPost]
    [RequireRole("Organiser")]
    public async Task<IActionResult> Create(int eventId, [FromBody] CreateCategoryDto dto)
    {
        var ev = await _db.Events.FindAsync(eventId);
        if (ev == null) return NotFound();
        if (ev.OrganiserId != HttpContext.Session.GetUserId()) return Forbid();

        var cat = new Category { Name = dto.Name, MinAge = dto.MinAge, MaxAge = dto.MaxAge, EventId = eventId };
        _db.Categories.Add(cat);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAll), new { eventId },
            new CategoryResponseDto(cat.CategoryId, cat.Name, cat.MinAge, cat.MaxAge, cat.EventId));
    }

    [HttpPut("{id:int}")]
    [RequireRole("Organiser")]
    public async Task<IActionResult> Update(int eventId, int id, [FromBody] UpdateCategoryDto dto)
    {
        var cat = await _db.Categories
            .Include(c => c.Event)
            .FirstOrDefaultAsync(c => c.CategoryId == id && c.EventId == eventId);
        if (cat == null) return NotFound();
        if (cat.Event.OrganiserId != HttpContext.Session.GetUserId()) return Forbid();

        cat.Name = dto.Name;
        cat.MinAge = dto.MinAge;
        cat.MaxAge = dto.MaxAge;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [RequireRole("Organiser")]
    public async Task<IActionResult> Delete(int eventId, int id)
    {
        var cat = await _db.Categories
            .Include(c => c.Event)
            .FirstOrDefaultAsync(c => c.CategoryId == id && c.EventId == eventId);
        if (cat == null) return NotFound();
        if (cat.Event.OrganiserId != HttpContext.Session.GetUserId()) return Forbid();

        _db.Categories.Remove(cat);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
