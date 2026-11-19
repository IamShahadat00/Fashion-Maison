namespace Maison.Domain.Enums
{
    public static class RoleNames
    {
        public const string SystemAdmin = "System Admin";
        public const string ProductManager = "Product Manager";
        public const string MarketingSales = "Marketing & Sales";
        public const string OperationsManager = "Operations Manager";
        public const string CustomerService = "Customer Service";
        public const string AuditOfficer = "Audit Officer";
        public const string HRManager = "HR Manager";
        public const string Customer = "Customer";
    }

    

    public enum EscalationType
    {
        RefundOverride,
        ManualInventoryOverride,
        UserAccountCreation,
        BroadcastRelease,
        RoleChange
    }
}