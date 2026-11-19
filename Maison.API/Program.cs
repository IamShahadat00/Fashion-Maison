using System.Text.Json.Serialization;
using Maison.API.Extensions;
using Maison.API.Middlewares;
using Maison.BLL.Interfaces;
using Maison.DAL.Context;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCleanArchitecture(builder.Configuration);

// Register Layered Core Services
builder.Services.AddMaisonCoreServices(builder.Configuration);

// Cross-Origin Requests for Next.js 15
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowNextFrontend", policy =>
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Auto-seed Initial Super Administrator
using (var scope = app.Services.CreateScope())
{
    var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
     var context = scope.ServiceProvider.GetRequiredService<MaisonDbContext>();
    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
    await authService.SeedSuperAdminAsync();
}

app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowNextFrontend");

app.UseAuthentication();
app.UseAuthorization();

// Log authenticated mutations
app.UseMiddleware<AuditLoggingMiddleware>();

app.MapControllers();

app.Run();