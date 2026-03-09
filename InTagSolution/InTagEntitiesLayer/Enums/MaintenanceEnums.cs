namespace InTagEntitiesLayer.Enums
{
    public enum WorkOrderStatus
    {
        Draft = 0,
        Open = 1,
        Assigned = 2,
        InProgress = 3,
        OnHold = 4,
        Completed = 5,
        Closed = 6,
        Cancelled = 7
    }

    public enum WorkOrderType
    {
        Corrective = 0,
        Preventive = 1,
        Predictive = 2,
        Emergency = 3,
        Inspection = 4
    }

    public enum WorkOrderPriority
    {
        Low = 0,
        Medium = 1,
        High = 2,
        Critical = 3
    }

    public enum PMScheduleFrequency
    {
        Daily = 1,
        Weekly = 7,
        Biweekly = 14,
        Monthly = 30,
        Quarterly = 90,
        SemiAnnual = 180,
        Annual = 365
    }

    public enum PMTriggerType
    {
        Calendar = 0,
        MeterBased = 1,
        ConditionBased = 2
    }

    public enum FailureType
    {
        Mechanical = 0,
        Electrical = 1,
        Pneumatic = 2,
        Hydraulic = 3,
        Software = 4,
        Operator = 5,
        Environmental = 6,
        WearAndTear = 7,
        Unknown = 8
    }
}
