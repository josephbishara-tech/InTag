namespace InTagViewModelLayer.Manufacturing
{
    public class ScheduleResultVm
    {
        public string OrderNumber { get; set; } = null!;
        public DateTimeOffset PlannedStart { get; set; }
        public DateTimeOffset PlannedEnd { get; set; }
        public int TotalDays { get; set; }
        public IReadOnlyList<ScheduledOperationVm> Operations { get; set; } = new List<ScheduledOperationVm>();
        public bool HasOverloadedWorkCenters { get; set; }
    }

    public class ScheduledOperationVm
    {
        public int Sequence { get; set; }
        public string OperationName { get; set; } = null!;
        public string WorkCenterCode { get; set; } = null!;
        public string WorkCenterName { get; set; } = null!;
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EndDate { get; set; }
        public decimal TotalMinutes { get; set; }
        public int DaysRequired { get; set; }
        public decimal WorkCenterUtilization { get; set; }
        public bool IsOverloaded { get; set; }
    }

    public class WorkCenterCapacityVm
    {
        public int WorkCenterId { get; set; }
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public decimal TotalCapacityHours { get; set; }
        public decimal LoadedHours { get; set; }
        public decimal AvailableHours { get; set; }
        public decimal Utilization { get; set; }
        public string Status { get; set; } = null!;
    }
}
