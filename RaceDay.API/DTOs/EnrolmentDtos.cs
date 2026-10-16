using RaceDay.API.Models;

namespace RaceDay.API.DTOs;

public record CreateEnrolmentDto(int EventId, int CategoryId);

public record EnrolmentResponseDto(
    int EnrolmentId, int ParticipantId, string ParticipantName,
    int EventId, string EventName, int CategoryId, string CategoryName,
    DateTime EnrolmentDate, EnrolmentStatus Status);
