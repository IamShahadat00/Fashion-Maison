using System;

namespace Maison.BLL.DTOs
{
    namespace Maison.BLL.DTOs
{
    // Customer Self-Registration
    public record CustomerRegisterRequestDto(
        string FirstName,
        string LastName,
        string Email,
        string Password,
        string? PhoneNumber,
        string? Address,
        string? imageUrl
    );
    public record LoginRequestDto(
        string Email,
        string Password
    );
       public record AuthResponseDto(
        string Token,
        Guid UserId,
        string FirstName,
        string LastName,
        string Email,
        string Role,
        bool IsExecutive,
        string RedirectDestination,
        DateTime ExpiresAt
    );
    public record AdminCreateUserDto(string FirstName, string LastName, string Email, string Password, int RoleId, string? PhoneNumber);
    public record UpdateProfileDto(string FirstName, string LastName, string? PhoneNumber, string? Address);
}
}