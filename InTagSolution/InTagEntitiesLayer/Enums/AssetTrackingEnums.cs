namespace InTagEntitiesLayer.Enums
{
    public enum TrackingRequestStatus
    {
        Draft = 0,
        Open = 1,
        InProgress = 2,
        Completed = 3,
        Cancelled = 4
    }

    public enum TrackingLineStatus
    {
        Pending = 0,
        Found = 1,
        Missing = 2,
        Relocated = 3,
        Damaged = 4
    }
}
