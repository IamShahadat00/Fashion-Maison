using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Maison.BLL.DTOs;
using Maison.BLL.Interfaces;
using Maison.DAL.Context;
using Maison.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Maison.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly MaisonDbContext _context;

        public AuthController(IAuthService authService, MaisonDbContext context)
        {
            _authService = authService;
            _context = context;
        }

        [HttpPost("register")]
          [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] CustomerRegisterRequestDto request)
        {
            if (request == null)
            {
                return BadRequest(new { message = "Request payload cannot be empty." });
            }

            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "Email and Password are required." });
            }

            try
            {
                var response = await _authService.RegisterCustomerAsync(request);
                return StatusCode(StatusCodes.Status201Created, response);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "Email and Password must be provided." });
            }

            try
            {
                var response = await _authService.LoginAsync(request);
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status401Unauthorized, new { message = ex.Message });
            }
        }
        [Authorize(Roles = UserRoleType.SystemAdmin)]
        [HttpPost("admin/create-user")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AdminCreateStaff([FromBody] AdminCreateUserDto dto)
        {
            var adminId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _authService.AdminCreateUserAsync(dto, adminId);
            return Ok(new { user.Id, user.FullName, user.Email, user.RoleId });
        }

        [Authorize(Roles = $"{UserRoleType.SystemAdmin},{UserRoleType.AuditOfficer},{UserRoleType.HRManager}")]
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.Users.Include(u => u.Role)
                .Select(u => new { u.Id, u.FullName, u.Email, Role = u.Role.Name, u.IsActive, u.CreatedAt })
                .ToListAsync();
            return Ok(users);
        }
    }
}