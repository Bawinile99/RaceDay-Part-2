namespace RaceDay.API.DTOs;

public record CreateResultDto(int EnrolmentId, int FinishTimeSeconds, int FinishingPosition);

public record ResultResponseDto(
    int ResultId, int EnrolmentId, string ParticipantName, string EventName,
    string CategoryName, int FinishTimeSeconds, int FinishingPosition,
    DateTime CapturedDate, string FormattedTime);
