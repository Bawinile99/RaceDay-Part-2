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
