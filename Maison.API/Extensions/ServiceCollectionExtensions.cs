using System.Text;
using Maison.BLL.Interfaces;
using Maison.BLL.Services;
using Maison.DAL.Context;
using Maison.Domain.Interfaces;
using Maison.DAL.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Maison.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMaisonCoreServices(this IServiceCollection services, IConfiguration config)
        {
            // Database
            services.AddDbContext<MaisonDbContext>(options =>
                options.UseNpgsql(config.GetConnectionString("DefaultConnection")));

            // Unit of work & Repositories
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Business Layer Services
            services.AddScoped<IAuditService, AuditService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IWorkflowApprovalService, WorkflowApprovalService>();

            // JWT Security
            var jwtKey = config["Jwt:Key"] ?? "MaisonDeElegance_SuperSecret_2026_Enterprise_Key_MustBeLong!";
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = config["Jwt:Issuer"] ?? "MaisonAPI",
                        ValidAudience = config["Jwt:Audience"] ?? "MaisonClient",
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
                    };
                });

            services.AddAuthorization();

            return services;
        }
    }
}