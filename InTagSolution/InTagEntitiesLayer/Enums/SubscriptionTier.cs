namespace InTagEntitiesLayer.Enums
{
    public enum SubscriptionTier
    {
        Starter = 0,       // 2 modules
        Professional = 1,  // 4 modules
        Enterprise = 2     // All modules + customization
    }

    public enum TenantStatus
    {
        Active = 0,
        Suspended = 1,
        Cancelled = 2,
        Trial = 3
    }

    public enum TenantIsolationStrategy
    {
        SharedSchema = 0,     // Starter / Professional
        SeparateDatabase = 1  // Enterprise
    }
}