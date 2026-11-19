using System.Threading.Tasks;
using Maison.DAL.Context;
using Maison.Domain.Interfaces;


namespace Maison.DAL.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly MaisonDbContext _context;

        public UnitOfWork(MaisonDbContext context)
        {
            _context = context;
        }

        public async Task<int> CommitAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}