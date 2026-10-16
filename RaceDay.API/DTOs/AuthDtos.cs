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
