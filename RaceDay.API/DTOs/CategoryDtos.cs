namespace RaceDay.API.DTOs;

public record CreateCategoryDto(string Name, int? MinAge, int? MaxAge);
public record UpdateCategoryDto(string Name, int? MinAge, int? MaxAge);
public record CategoryResponseDto(int CategoryId, string Name, int? MinAge, int? MaxAge, int EventId);
