using System.ComponentModel.DataAnnotations;
using InTagEntitiesLayer.Common;

namespace InTagEntitiesLayer.Maintenance
{
    public class WorkOrderLabor : BaseEntity
    {
        [Required]
        public int WorkOrderId { get; set; }

        [Required]
        public Guid TechnicianUserId { get; set; }

        [MaxLength(200)]
        public string? TechnicianName { get; set; }

        [Required]
        public DateTimeOffset StartTime { get; set; }

        public DateTimeOffset? EndTime { get; set; }

        public decimal HoursWorked { get; set; }

        public decimal HourlyRate { get; set; }

        public decimal Cost => HoursWorked * HourlyRate;

        [MaxLength(1000)]
        public string? WorkPerformed { get; set; }

        // Navigation
        public WorkOrder WorkOrder { get; set; } = null!;
    }
}
