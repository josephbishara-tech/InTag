namespace InTagEntitiesLayer.Enums
{
    public enum ProductionOrderStatus
    {
        Draft = 0,
        Planned = 1,
        Released = 2,
        InProgress = 3,
        Completed = 4,
        Closed = 5,
        Cancelled = 6
    }

    public enum ProductionPriority
    {
        Low = 0,
        Normal = 1,
        High = 2,
        Urgent = 3
    }

    public enum BOMStatus
    {
        Draft = 0,
        Active = 1,
        Obsolete = 2
    }

    public enum WorkCenterStatus
    {
        Active = 0,
        Maintenance = 1,
        Inactive = 2
    }

    public enum LotBatchStatus
    {
        Created = 0,
        InProcess = 1,
        Quarantine = 2,
        Released = 3,
        Rejected = 4,
        Expired = 5
    }

    public enum QualityCheckResult
    {
        Pending = 0,
        Pass = 1,
        Fail = 2,
        ConditionalPass = 3
    }

    public enum UnitOfMeasure
    {
        Each = 0,
        Kg = 1,
        Gram = 2,
        Liter = 3,
        Meter = 4,
        SqMeter = 5,
        CubicMeter = 6,
        Ton = 7,
        Piece = 8,
        Box = 9,
        Pallet = 10,
        Roll = 11,
        Sheet = 12,
        Gallon = 13,
        Pound = 14,
        Foot = 15
    }
}
