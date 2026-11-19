using Maison.Domain.Entities;

namespace Maison.BLL.Interfaces
{
    public interface IRedirectRoutingService
    {
        bool IsExecutiveEmailDomain(string email);
        string DetermineDestination(User user);
    }
}