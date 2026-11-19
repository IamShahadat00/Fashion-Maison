namespace Maison.Domain.Enums
{
    public static class UserRoleType
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

    public enum RiskTier
    {
        Tier1_LowRisk_AutoPublish,
        Tier2_MediumRisk_AdminOnly,
        Tier3_HighRisk_DualApproval
    }

    public enum PublishingActionType
    {
        BasePriceIncrease,
        BasePriceReduction,
        PromotionalDiscount,
        ClearanceSale,
        NewProductLaunch,
        CustomerRefund
    }
}