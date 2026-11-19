using System.Text;
using Maison.Application.Services;
using Maison.DAL.Context;
using Maison.Domain.Interfaces;
using Maison.BLL.Interfaces;
using Maison.BLL.Services;
using Maison.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Maison.DAL.Repositories;
using Maison.DAL.Security;

namespace Maison.API.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddCleanArchitecture(this IServiceCollection services, IConfiguration configuration)
        {
            // 1. Persistence & DbContext
            services.AddDbContext<MaisonDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

            // 2. Domain & Application Repositories
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // 3. Security Services
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IRedirectionRoutingService, RedirectionRoutingService>();

            // 4. Application Use Case Services
            services.AddScoped<IAuthService, AuthService>();

            // 5. JWT Authentication
            var jwtKey = configuration["Jwt:Key"] ?? "MaisonDeElegance_SuperSecret_2026_Enterprise_Key_MustBeLong!";
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = configuration["Jwt:Issuer"] ?? "MaisonAPI",
                        ValidAudience = configuration["Jwt:Audience"] ?? "MaisonClient",
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
                    };
                });

            services.AddAuthorization();
            return services;
        }
    }
}