using InTagEntitiesLayer.Enums;

namespace InTagViewModelLayer.Workflow
{
    public class NotificationVm
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Message { get; set; }
        public string? ActionUrl { get; set; }
        public bool IsRead { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public NotificationChannel Channel { get; set; }
    }
}
