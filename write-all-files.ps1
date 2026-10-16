# write-all-files.ps1
# Run from solution root: C:\Users\Student\source\repos\RaceDay

$ErrorActionPreference = "Stop"

# ============================================================
# 1. MODELS
# ============================================================

@'
using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Models;

public enum UserRole { Organiser, Participant }

public class User
{
    public int UserId { get; set; }

    [Required, MaxLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    public UserRole Role { get; set; }

    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public string? ProfilePictureUrl { get; set; }

    public ICollection<Event> OrganisedEvents { get; set; } = new List<Event>();
    public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
}
'@ | Set-Content -Path "RaceDay.API\Models\User.cs" -Encoding UTF8

@'
using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Models;

public enum EventType { Run, Walk, Cycle }

public class Event
{
    public int EventId { get; set; }

    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public DateTime EventDate { get; set; }

    [Required, MaxLength(200)]
    public string Location { get; set; } = string.Empty;

    [Required]
    public double Distance { get; set; }

    [Required]
    public EventType EventType { get; set; }

    public string? BannerImageUrl { get; set; }

    public int OrganiserId { get; set; }
    public User Organiser { get; set; } = null!;

    public ICollection<Category> Categories { get; set; } = new List<Category>();
    public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
}
'@ | Set-Content -Path "RaceDay.API\Models\Event.cs" -Encoding UTF8

@'
using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Models;

public class Category
{
    public int CategoryId { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public int? MinAge { get; set; }
    public int? MaxAge { get; set; }

    public int EventId { get; set; }
    public Event Event { get; set; } = null!;

    public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
}
'@ | Set-Content -Path "RaceDay.API\Models\Category.cs" -Encoding UTF8

@'
using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Models;

public enum EnrolmentStatus { Confirmed, Pending, Cancelled }

public class Enrolment
{
    public int EnrolmentId { get; set; }

    public int ParticipantId { get; set; }
    public User Participant { get; set; } = null!;

    public int EventId { get; set; }
    public Event Event { get; set; } = null!;

    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public DateTime EnrolmentDate { get; set; } = DateTime.UtcNow;

    [Required]
    public EnrolmentStatus Status { get; set; } = EnrolmentStatus.Confirmed;

    public Result? Result { get; set; }
}
'@ | Set-Content -Path "RaceDay.API\Models\Enrolment.cs" -Encoding UTF8

@'
namespace RaceDay.API.Models;

public class Result
{
    public int ResultId { get; set; }

    public int EnrolmentId { get; set; }
    public Enrolment Enrolment { get; set; } = null!;

    public int FinishTimeSeconds { get; set; }

    public int FinishingPosition { get; set; }

    public DateTime CapturedDate { get; set; } = DateTime.UtcNow;
}
'@ | Set-Content -Path "RaceDay.API\Models\Result.cs" -Encoding UTF8

# ============================================================
# 2. DATA
# ============================================================

@'
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Models;

namespace RaceDay.API.Data;

public class RaceDayDbContext : DbContext
{
    public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Enrolment> Enrolments => Set<Enrolment>();
    public DbSet<Result> Results => Set<Result>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();

        modelBuilder.Entity<Event>()
            .HasOne(e => e.Organiser)
            .WithMany(u => u.OrganisedEvents)
            .HasForeignKey(e => e.OrganiserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Category>()
            .HasOne(c => c.Event)
            .WithMany(e => e.Categories)
            .HasForeignKey(c => c.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Enrolment>()
            .HasOne(en => en.Participant)
            .WithMany(u => u.Enrolments)
            .HasForeignKey(en => en.ParticipantId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Enrolment>()
            .HasOne(en => en.Event)
            .WithMany(e => e.Enrolments)
            .HasForeignKey(en => en.EventId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Enrolment>()
            .HasOne(en => en.Category)
            .WithMany(c => c.Enrolments)
            .HasForeignKey(en => en.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Result>().HasIndex(r => r.EnrolmentId).IsUnique();

        modelBuilder.Entity<Result>()
            .HasOne(r => r.Enrolment)
            .WithOne(en => en.Result)
            .HasForeignKey<Result>(r => r.EnrolmentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Enrolment>()
            .HasIndex(e => new { e.ParticipantId, e.EventId })
            .IsUnique();
    }
}
'@ | Set-Content -Path "RaceDay.API\Data\RaceDayDbContext.cs" -Encoding UTF8

# ============================================================
# 3. SERVICES
# ============================================================

@'
namespace RaceDay.API.Services;

public interface IRaceDayPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}
'@ | Set-Content -Path "RaceDay.API\Services\IRaceDayPasswordHasher.cs" -Encoding UTF8

@'
namespace RaceDay.API.Services;

public class BcryptPasswordHasher : IRaceDayPasswordHasher
{
    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password);
    public bool Verify(string password, string hash) => BCrypt.Net.BCrypt.Verify(password, hash);
}
'@ | Set-Content -Path "RaceDay.API\Services\BcryptPasswordHasher.cs" -Encoding UTF8

@'
namespace RaceDay.API.Services;

public static class SessionExtensions
{
    private const string UserIdKey = "UserId";
    private const string RoleKey = "Role";

    public static void SetUser(this ISession session, int userId, string role)
    {
        session.SetInt32(UserIdKey, userId);
        session.SetString(RoleKey, role);
    }

    public static int? GetUserId(this ISession session) => session.GetInt32(UserIdKey);
    public static string? GetRole(this ISession session) => session.GetString(RoleKey);
    public static bool IsAuthenticated(this ISession session) => session.GetUserId() != null;
    public static bool IsInRole(this ISession session, string role) => session.GetRole() == role;
}
'@ | Set-Content -Path "RaceDay.API\Services\SessionExtensions.cs" -Encoding UTF8

# ============================================================
# 4. ATTRIBUTES
# ============================================================

@'
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RaceDay.API.Services;

namespace RaceDay.API.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireRoleAttribute : Attribute, IAuthorizationFilter
{
    private readonly string[] _roles;

    public RequireRoleAttribute(params string[] roles) => _roles = roles;

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var session = context.HttpContext.Session;

        if (!session.IsAuthenticated())
        {
            context.Result = new UnauthorizedObjectResult(new { message = "Not authenticated." });
            return;
        }

        if (_roles.Length > 0 && !_roles.Contains(session.GetRole()))
        {
            context.Result = new ObjectResult(new { message = "Forbidden." }) { StatusCode = 403 };
        }
    }
}
'@ | Set-Content -Path "RaceDay.API\Attributes\RequireRoleAttribute.cs" -Encoding UTF8

# ============================================================
# 5. DTOs
# ============================================================

@'
using RaceDay.API.Models;

namespace RaceDay.API.DTOs;

public record RegisterDto(
    string FirstName, string LastName, string Email,
    string Password, UserRole Role,
    string? PhoneNumber, DateTime? DateOfBirth);

public record LoginDto(string Email, string Password);

public record UserResponseDto(
    int UserId, string FirstName, string LastName, string Email,
    UserRole Role, string? PhoneNumber, DateTime? DateOfBirth,
    string? ProfilePictureUrl);

public record UpdateProfileDto(
    string FirstName, string LastName,
    string? PhoneNumber, DateTime? DateOfBirth, string? ProfilePictureUrl);
'@ | Set-Content -Path "RaceDay.API\DTOs\AuthDtos.cs" -Encoding UTF8

@'
using RaceDay.API.Models;

namespace RaceDay.API.DTOs;

public record CreateEventDto(
    string Name, string Description, DateTime EventDate,
    string Location, double Distance, EventType EventType,
    string? BannerImageUrl);

public record UpdateEventDto(
    string Name, string Description, DateTime EventDate,
    string Location, double Distance, EventType EventType,
    string? BannerImageUrl);

public record EventResponseDto(
    int EventId, string Name, string Description, DateTime EventDate,
    string Location, double Distance, EventType EventType,
    string? BannerImageUrl, int OrganiserId, string OrganiserName,
    int EnrolmentCount);
'@ | Set-Content -Path "RaceDay.API\DTOs\EventDtos.cs" -Encoding UTF8

@'
namespace RaceDay.API.DTOs;

public record CreateCategoryDto(string Name, int? MinAge, int? MaxAge);
public record UpdateCategoryDto(string Name, int? MinAge, int? MaxAge);
public record CategoryResponseDto(int CategoryId, string Name, int? MinAge, int? MaxAge, int EventId);
'@ | Set-Content -Path "RaceDay.API\DTOs\CategoryDtos.cs" -Encoding UTF8

@'
using RaceDay.API.Models;

namespace RaceDay.API.DTOs;

public record CreateEnrolmentDto(int EventId, int CategoryId);

public record EnrolmentResponseDto(
    int EnrolmentId, int ParticipantId, string ParticipantName,
    int EventId, string EventName, int CategoryId, string CategoryName,
    DateTime EnrolmentDate, EnrolmentStatus Status);
'@ | Set-Content -Path "RaceDay.API\DTOs\EnrolmentDtos.cs" -Encoding UTF8

@'
namespace RaceDay.API.DTOs;

public record CreateResultDto(int EnrolmentId, int FinishTimeSeconds, int FinishingPosition);

public record ResultResponseDto(
    int ResultId, int EnrolmentId, string ParticipantName, string EventName,
    string CategoryName, int FinishTimeSeconds, int FinishingPosition,
    DateTime CapturedDate, string FormattedTime);
'@ | Set-Content -Path "RaceDay.API\DTOs\ResultDtos.cs" -Encoding UTF8

# ============================================================
# 6. CONTROLLERS
# ============================================================

@'
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
'@ | Set-Content -Path "RaceDay.API\Controllers\AuthController.cs" -Encoding UTF8

@'
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
'@ | Set-Content -Path "RaceDay.API\Controllers\ProfileController.cs" -Encoding UTF8

@'
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
'@ | Set-Content -Path "RaceDay.API\Controllers\EventsController.cs" -Encoding UTF8

@'
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
'@ | Set-Content -Path "RaceDay.API\Controllers\CategoriesController.cs" -Encoding UTF8

@'
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
'@ | Set-Content -Path "RaceDay.API\Controllers\EnrolmentsController.cs" -Encoding UTF8

@'
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
'@ | Set-Content -Path "RaceDay.API\Controllers\ResultsController.cs" -Encoding UTF8

# ============================================================
# 7. PROGRAM.CS + APPSETTINGS
# ============================================================

@'
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<RaceDayDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IRaceDayPasswordHasher, BcryptPasswordHasher>();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".RaceDay.Session";
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<RaceDayDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseSession();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program { }
'@ | Set-Content -Path "RaceDay.API\Program.cs" -Encoding UTF8

@'
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=raceday.db"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
'@ | Set-Content -Path "RaceDay.API\appsettings.json" -Encoding UTF8

Write-Host ""
Write-Host "==========================================" -ForegroundColor Green
Write-Host " All files written successfully!" -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "  1. dotnet build" -ForegroundColor White
Write-Host "  2. dotnet ef migrations add InitialCreate --project RaceDay.API" -ForegroundColor White
Write-Host "  3. dotnet ef database update --project RaceDay.API" -ForegroundColor White
Write-Host "  4. dotnet run --project RaceDay.API" -ForegroundColor White