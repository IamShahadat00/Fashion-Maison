using System;
using System.Threading.Tasks;
using Maison.Domain.Entities;

namespace Maison.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid id);
        Task<User?> GetByEmailAsync(string email);
        Task<bool> EmailExistsAsync(string email);
        Task AddAsync(User user);
        void Update(User user);
    }
}