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
