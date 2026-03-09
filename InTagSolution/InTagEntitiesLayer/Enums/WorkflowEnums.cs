namespace InTagEntitiesLayer.Enums
{
    public enum WorkflowStatus
    {
        Draft = 0,
        Active = 1,
        Inactive = 2
    }

    public enum WorkflowInstanceStatus
    {
        Pending = 0,
        InProgress = 1,
        Completed = 2,
        Rejected = 3,
        Cancelled = 4
    }

    public enum StepType
    {
        Approval = 0,
        Review = 1,
        Notification = 2,
        Condition = 3
    }

    public enum StepExecutionMode
    {
        Sequential = 0,
        Parallel = 1
    }

    public enum StepActionResult
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2,
        Delegated = 3,
        Escalated = 4,
        Skipped = 5
    }

    public enum NotificationChannel
    {
        InApp = 0,
        Email = 1,
        Both = 2
    }

    public enum WorkflowCategory
    {
        AssetTransfer = 0,
        AssetDisposal = 1,
        DocumentApproval = 2,
        PurchaseRequisition = 3,
        ProductionRelease = 4,
        WorkOrderApproval = 5,
        ChangeRequest = 6,
        General = 7
    }
}
