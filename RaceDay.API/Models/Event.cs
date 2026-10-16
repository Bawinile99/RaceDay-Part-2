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
