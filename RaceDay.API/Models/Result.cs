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
