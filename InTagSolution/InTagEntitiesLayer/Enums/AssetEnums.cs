namespace InTagEntitiesLayer.Enums
{
    public enum AssetStatus
    {
        Draft = 0,
        Acquired = 1,
        Commissioned = 2,
        Operational = 3,
        UnderMaintenance = 4,
        Decommissioned = 5,
        Disposed = 6
    }

    public enum AssetCategory
    {
        Equipment = 0,
        Facility = 1,
        Vehicle = 2,
        Tool = 3,
        ITAsset = 4,
        Furniture = 5,
        Infrastructure = 6
    }

    public enum DepreciationMethod
    {
        StraightLine = 0,
        DecliningBalance = 1,
        SumOfYearsDigits = 2,
        UnitsOfProduction = 3
    }

    public enum ConditionRating
    {
        Excellent = 5,
        Good = 4,
        Fair = 3,
        Poor = 2,
        Critical = 1
    }

    public enum TransferStatus
    {
        Pending = 0,
        Approved = 1,
        InTransit = 2,
        Completed = 3,
        Rejected = 4
    }
}
