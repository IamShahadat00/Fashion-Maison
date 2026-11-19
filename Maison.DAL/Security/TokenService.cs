using System.Security.Claims;
using System.Text;
using Maison.Domain.Entities;
using Microsoft.Extensions.Configuration;

namespace Maison.Infrastructure.Security
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;

        public TokenService(IConfiguration config)
        {
            _config = config;
        }

        public (string Token, DateTime ExpiresAt) GenerateJwtToken(User user, string destination)
        {
            var jwtKey = _config["Jwt:Key"] ?? "MaisonDeElegance_SuperSecret_2026_Enterprise_Key_MustBeLong!";
            var issuer = _config["Jwt:Issuer"] ?? "MaisonAPI";
            var audience = _config["Jwt:Audience"] ?? "MaisonClient";
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiresAt = DateTime.UtcNow.AddHours(12);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Role, user.Role?.Name ?? "Customer"),
                new Claim("is_executive", (user.Role?.IsExecutive ?? false).ToString().ToLower()),
                new Claim("redirect_destination", destination),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expiresAt,
                signingCredentials: creds
            );

            return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
        }
    }
}