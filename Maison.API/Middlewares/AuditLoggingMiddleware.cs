using System.Security.Claims;
using System.Threading.Tasks;
using Maison.BLL.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Maison.API.Middlewares
{
    public class AuditLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public AuditLoggingMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext context, IAuditService auditService)
        {
            await _next(context);

            // Audit write mutations (POST, PUT, DELETE) executed by authenticated staff
            if (context.Request.Method != "GET" && context.User.Identity?.IsAuthenticated == true)
            {
                var email = context.User.FindFirstValue(ClaimTypes.Email) ?? "Unknown";
                var role = context.User.FindFirstValue(ClaimTypes.Role) ?? "Unknown";
                var path = context.Request.Path;
                var method = context.Request.Method;

                await auditService.LogActionAsync(
                    null,
                    email,
                    role,
                    $"{method}_CALL",
                    "HTTP_TRAFFIC",
                    path,
                    $"HTTP {context.Response.StatusCode}"
                );
            }
        }
    }
}