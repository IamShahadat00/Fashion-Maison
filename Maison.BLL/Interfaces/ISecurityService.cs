using System;
using Maison.Domain.Entities;

namespace Maison.BLL.Interfaces
{
    public interface IPasswordHasher
    {
        string HashPassword(string password);
        bool VerifyPassword(string password, string passwordHash);
    }

    public interface ITokenService
    {
        (string Token, DateTime ExpiresAt) GenerateJwtToken(User user, string destination);
    }
}