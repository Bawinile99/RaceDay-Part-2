namespace RaceDay.MVC.ViewModels;

// ---- AUTH / USER ----
public class LoginVm
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
}

public class RegisterVm
{
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public string Role { get; set; } = "Participant"; // "Organiser" | "Participant"
    public string? PhoneNumber { get; set; }
    public DateTime? DateOfBirth { get; set; }
}

public class UserVm
{
    public int UserId { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Role { get; set; } = "";
    public string? PhoneNumber { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? ProfilePictureUrl { get; set; }
}

public class UpdateProfileVm
{
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string? PhoneNumber { get; set; }
    public DateTime? DateOfBirth { get; set; }
}

// ---- EVENTS ----
public class EventVm
{
    public int EventId { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public DateTime EventDate { get; set; }
    public string Location { get; set; } = "";
    public double Distance { get; set; }
    public string EventType { get; set; } = "";
    public string? BannerImageUrl { get; set; }
    public int OrganiserId { get; set; }
    public string OrganiserName { get; set; } = "";
    public int EnrolmentCount { get; set; }
}

public class CreateEventVm
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public DateTime EventDate { get; set; } = DateTime.Today.AddDays(30);
    public string Location { get; set; } = "";
    public double Distance { get; set; }
    public string EventType { get; set; } = "Run";
    public string? BannerImageUrl { get; set; }
}

// ---- CATEGORIES ----
public class CategoryVm
{
    public int CategoryId { get; set; }
    public string Name { get; set; } = "";
    public int? MinAge { get; set; }
    public int? MaxAge { get; set; }
    public int EventId { get; set; }
}

public class CreateCategoryVm
{
    public string Name { get; set; } = "";
    public int? MinAge { get; set; }
    public int? MaxAge { get; set; }
}

// ---- ENROLMENTS ----
public class EnrolmentVm
{
    public int EnrolmentId { get; set; }
    public int ParticipantId { get; set; }
    public string ParticipantName { get; set; } = "";
    public int EventId { get; set; }
    public string EventName { get; set; } = "";
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = "";
    public DateTime EnrolmentDate { get; set; }
    public string Status { get; set; } = "";
}

public class CreateEnrolmentVm
{
    public int EventId { get; set; }
    public int CategoryId { get; set; }
}

// ---- RESULTS ----
public class ResultVm
{
    public int ResultId { get; set; }
    public int EnrolmentId { get; set; }
    public string ParticipantName { get; set; } = "";
    public string EventName { get; set; } = "";
    public string CategoryName { get; set; } = "";
    public int FinishTimeSeconds { get; set; }
    public int FinishingPosition { get; set; }
    public DateTime CapturedDate { get; set; }
    public string FinishTimeDisplay { get; set; } = "";
}

public class CreateResultVm
{
    public int EnrolmentId { get; set; }
    public int FinishTimeSeconds { get; set; }
    public int FinishingPosition { get; set; }
}
