namespace InTagEntitiesLayer.Enums
{
    public enum DocumentStatus
    {
        Draft = 0,
        InReview = 1,
        Approved = 2,
        Published = 3,
        Obsolete = 4,
        Archived = 5
    }

    public enum DocumentType
    {
        SOP = 0,
        WorkInstruction = 1,
        Contract = 2,
        EngineeringDrawing = 3,
        QualityRecord = 4,
        Form = 5,
        Template = 6,
        Policy = 7,
        Manual = 8,
        Report = 9
    }

    public enum DocumentCategory
    {
        Quality = 0,
        Safety = 1,
        Operations = 2,
        Engineering = 3,
        Maintenance = 4,
        HR = 5,
        Finance = 6,
        Regulatory = 7,
        General = 8
    }

    public enum ReviewCycle
    {
        Monthly = 1,
        Quarterly = 3,
        SemiAnnual = 6,
        Annual = 12,
        Biennial = 24,
        None = 0
    }

    public enum ApprovalAction
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2,
        ReturnedForRevision = 3
    }

    public enum DistributionMethod
    {
        InApp = 0,
        Email = 1,
        Print = 2,
        ExternalLink = 3
    }

    public enum FolderOwnerType
    {
        User = 0,
        Product = 1,
        Department = 2
    }

    public enum ShareTargetType
    {
        User = 0,
        Department = 1,
        Everyone = 2
    }

    public enum SharePermission
    {
        View = 0,
        Download = 1,
        Edit = 2
    }

}
