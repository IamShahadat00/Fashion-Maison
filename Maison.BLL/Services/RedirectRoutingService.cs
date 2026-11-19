using System;
using Maison.Application.Interfaces;
using Maison.Domain.Entities;
using Maison.Domain.Enums;

namespace Maison.Application.Services
{
    public class RedirectionRoutingService : IRedirectionRoutingService
    {
        // Enterprise corporate domain reserved strictly for internal staff/executives
        private const string CorporateDomain = "@maisondelegance.com";

        public bool IsExecutiveEmailDomain(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            return email.Trim().EndsWith(CorporateDomain, StringComparison.OrdinalIgnoreCase);
        }

        public string DetermineDestination(User user)
        {
            // Customers are routed to the public storefront landing experience
            if (user.Role == null || user.Role.Name == UserRoleType.Customer)
            {
                return "/"; // Landing Page Destination
            }

            // Executives bypass customer landing and are mapped strictly to their role dashboard
            return user.Role.Name switch
            {
                UserRoleType.SystemAdmin => "/dashboard/admin",
                UserRoleType.ProductManager => "/dashboard/product-manager",
                UserRoleType.MarketingSales => "/dashboard/marketing-sales",
                UserRoleType.OperationsManager => "/dashboard/operations",
                UserRoleType.CustomerService => "/dashboard/customer-service",
                UserRoleType.AuditOfficer => "/dashboard/audit-officer",
                UserRoleType.HRManager => "/dashboard/hr",
                _ => "/dashboard/executive"
            };
        }
    }
}