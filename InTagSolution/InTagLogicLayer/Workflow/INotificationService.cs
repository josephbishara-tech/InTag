using InTagViewModelLayer.Workflow;

namespace InTagLogicLayer.Workflow
{
    public interface INotificationService
    {
        Task SendAsync(Guid recipientUserId, string title, string? message, string? actionUrl,
            InTagEntitiesLayer.Enums.NotificationChannel channel, int? workflowInstanceId = null);
        Task<IReadOnlyList<NotificationVm>> GetForUserAsync(Guid userId, bool unreadOnly = false);
        Task<int> GetUnreadCountAsync(Guid userId);
        Task MarkAsReadAsync(int notificationId);
        Task MarkAllReadAsync(Guid userId);
    }
}
