using System;
using System.Threading.Tasks;

namespace Maison.Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        Task<int> CommitAsync();
    }
}