using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Maison.BLL.DTOs;
using Maison.BLL.Interfaces;
using Maison.DAL.Context;
using Maison.Domain.Entities;
using Maison.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Maison.Domain.Interfaces;


namespace Maison.BLL.Services
{
    public class AuthService : IAuthService
    {
        private readonly MaisonDbContext _context;
        private readonly IConfiguration _config;
        private readonly IAuditService _auditService;
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenService _tokenService;
        private readonly IRedirectRoutingService _routingService;

        public AuthService(MaisonDbContext context, IConfiguration config, IAuditService auditService,IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            IPasswordHasher passwordHasher,
            ITokenService tokenService,
            IRedirectRoutingService routingService)
        {
            _context = context;
            _config = config;
            _auditService = auditService;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
            _routingService = routingService;
        }

        public async Task SeedSuperAdminAsync()
        {
            const string email = "admin@maisondelegance.com";
            if (!await _context.Users.AnyAsync(u => u.Email == email))
            {
                var admin = new User
                {
                    FirstName = "System Admin",
                    LastName = "Maison",
                    Email = email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@2026"),
                    RoleId = 1, // System Admin
                    PhoneNumber = "+1-800-ELEGANCE"
                };
                _context.Users.Add(admin);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<AuthResponseDto> RegisterCustomerAsync(CustomerRegisterRequestDto request)
        {
            // 1. Validation & corporate domain safety check
            var normalizedEmail = request.Email.Trim().ToLowerInvariant();

            if (_routingService.IsExecutiveEmailDomain(normalizedEmail))
            {
                throw new InvalidOperationException("Registration with internal corporate executive email domains is restricted. Please contact IT Administration.");
            }

            if (await _userRepository.EmailExistsAsync(normalizedEmail))
            {
                throw new InvalidOperationException("An account with this email address already exists.");
            }

            // 2. Build Customer Entity (RoleId = 8: Customer)
            var user = new User
            {
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName?.Trim(),
                Email = normalizedEmail,
                PasswordHash = _passwordHasher.HashPassword(request.Password),
                PhoneNumber = request.PhoneNumber?.Trim(),
                Address = request.Address?.Trim(),
                RoleId = 8,
                IsActive = true,
                Role = new Role { Id = 8, Name = UserRoleType.Customer, IsExecutive = false }
            };

            await _userRepository.AddAsync(user);
            await _unitOfWork.CommitAsync();

            // 3. Routing Engine Evaluation: Customer -> Landing Page
            var destination = _routingService.DetermineDestination(user);
            var (token, expiresAt) = _tokenService.GenerateJwtToken(user, destination);

            return new AuthResponseDto(
                Token: token,
                UserId: user.Id,
                FirstName: user.FirstName,
                LastName: user.LastName,
                Email: user.Email,
                Role: user.Role.Name,
                IsExecutive: false,
                RedirectDestination: destination,
                ExpiresAt: expiresAt
            );
        }
     public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
        {
            var normalizedEmail = request.Email.Trim().ToLowerInvariant();
            var user = await _userRepository.GetByEmailAsync(normalizedEmail);

            // Guard: Invalid Credentials
            if (user == null || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            // Guard: Account Status
            if (!user.IsActive)
            {
                throw new UnauthorizedAccessException("This account has been deactivated. Please contact support.");
            }

            // Guard: Executive Domain Verification
            bool isExecutiveUser = user.Role.IsExecutive || user.Role.Name != UserRoleType.Customer;
            bool hasExecutiveEmailDomain = _routingService.IsExecutiveEmailDomain(user.Email);

            if (isExecutiveUser && !hasExecutiveEmailDomain)
            {
                throw new UnauthorizedAccessException("Executive security access violation: Missing verified corporate email domain.");
            }

            // Resolve Redirection Destination
            var destination = _routingService.DetermineDestination(user);
            var (token, expiresAt) = _tokenService.GenerateJwtToken(user, destination);

            return new AuthResponseDto(
                Token: token,
                UserId: user.Id,
                FirstName: user.FirstName,
                LastName: user.LastName,
                Email: user.Email,
                Role: user.Role.Name,
                IsExecutive: isExecutiveUser,
                RedirectDestination: destination,
                ExpiresAt: expiresAt
            );
        }

        public async Task<User> AdminCreateUserAsync(AdminCreateUserDto dto, Guid adminId)
        {
            if (await _context.Users.AnyAsync(u => u.Email.ToLower() == dto.Email.ToLower()))
                throw new InvalidOperationException("Email address already registered.");

            var user = new User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email.ToLower(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                RoleId = dto.RoleId,
                PhoneNumber = dto.PhoneNumber
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync(adminId, "admin@maisondelegance.com", UserRoleType.SystemAdmin, "STAFF_PROVISIONED", "User & Role Access Management", "/api/auth/admin/create-user", $"Created staff {user.Email} with RoleId {user.RoleId}");

            return user;
        }

        private AuthResponseDto GenerateAuthDto(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"] ?? "MaisonDeElegance_SuperSecret_2026_Enterprise_Key_MustBeLong!"));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                new Claim(ClaimTypes.Role, user.Role.Name)
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"] ?? "MaisonAPI",
                audience: _config["Jwt:Audience"] ?? "MaisonClient",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(14),
                signingCredentials: credentials
            );

            // Dynamic Dashboard Route by Role
            var dashboardRoute = user.Role.Name switch
            {
                UserRoleType.SystemAdmin => "/dashboard/admin",
                UserRoleType.ProductManager => "/dashboard/product-manager",
                UserRoleType.MarketingSales => "/dashboard/marketing-sales",
                UserRoleType.OperationsManager => "/dashboard/operations",
                UserRoleType.CustomerService => "/dashboard/customer-service",
                UserRoleType.AuditOfficer => "/dashboard/audit-officer",
                UserRoleType.HRManager => "/dashboard/hr",
                _ => "/dashboard/customer"
            };

            return new AuthResponseDto(
                new JwtSecurityTokenHandler().WriteToken(token),
                user.Id,
                user.FirstName,
                user.LastName,
                user.Email,
                user.Role.Name,
                dashboardRoute
            );
        }
    }

}