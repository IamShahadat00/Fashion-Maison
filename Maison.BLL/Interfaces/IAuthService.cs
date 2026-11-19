using System;
using System.Threading.Tasks;
using Maison.BLL.DTOs;
using Maison.Domain.Entities;

namespace Maison.BLL.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterCustomerAsync(CustomerRegisterRequestDto request);
        Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
        Task<User> AdminCreateUserAsync(AdminCreateUserDto dto, Guid adminId);
        Task SeedSuperAdminAsync();
    }
}