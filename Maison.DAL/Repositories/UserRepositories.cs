using System;
using System.Threading.Tasks;
using Maison.Domain.Entities;
using Maison.Domain.Enums;
using Maison.Domain.Interfaces;
using Maison.DAL.Context;
using Microsoft.EntityFrameworkCore;

namespace Maison.DAL.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly MaisonDbContext _context;

        public UserRepository(MaisonDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            return await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task AddAsync(User user)
        {
            await _context.Users.AddAsync(user);
        }

        public void Update(User user)
        {
            _context.Users.Update(user);
        }
    }
}