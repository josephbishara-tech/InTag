using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using InTagEntitiesLayer.Enums;
using InTagEntitiesLayer.Workflow;
using InTagRepositoryLayer.Common;
using InTagViewModelLayer.Workflow;

namespace InTagLogicLayer.Workflow
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _uow;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(IUnitOfWork uow, ILogger<NotificationService> logger)
        {
            _uow = uow;
            _logger = logger;
        }

        public async Task SendAsync(Guid recipientUserId, string title, string? message,
            string? actionUrl, NotificationChannel channel, int? workflowInstanceId = null)
        {
            var notification = new Notification
            {
                RecipientUserId = recipientUserId,
                Title = title,
                Message = message,
                ActionUrl = actionUrl,
                Channel = channel,
                WorkflowInstanceId = workflowInstanceId
            };

            await _uow.Notifications.AddAsync(notification);
            await _uow.SaveChangesAsync();

            // Email sending placeholder — integrate SMTP/SendGrid in production
            if (channel == NotificationChannel.Email || channel == NotificationChannel.Both)
            {
                _logger.LogInformation("[Notification] Email queued for user {UserId}: {Title}", recipientUserId, title);
                notification.IsEmailSent = true;
                notification.EmailSentDate = DateTimeOffset.UtcNow;
                _uow.Notifications.Update(notification);
                await _uow.SaveChangesAsync();
            }
        }

        public async Task<IReadOnlyList<NotificationVm>> GetForUserAsync(Guid userId, bool unreadOnly = false)
        {
            var query = _uow.Notifications.Query()
                .Where(n => n.RecipientUserId == userId);

            if (unreadOnly) query = query.Where(n => !n.IsRead);

            return await query.OrderByDescending(n => n.CreatedDate).Take(50)
                .Select(n => new NotificationVm
                {
                    Id = n.Id,
                    Title = n.Title,
                    Message = n.Message,
                    ActionUrl = n.ActionUrl,
                    IsRead = n.IsRead,
                    CreatedDate = n.CreatedDate,
                    Channel = n.Channel
                }).ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(Guid userId)
            => await _uow.Notifications.Query()
                .CountAsync(n => n.RecipientUserId == userId && !n.IsRead);

        public async Task MarkAsReadAsync(int notificationId)
        {
            var n = await _uow.Notifications.GetByIdAsync(notificationId);
            if (n == null) return;
            n.IsRead = true;
            n.ReadDate = DateTimeOffset.UtcNow;
            _uow.Notifications.Update(n);
            await _uow.SaveChangesAsync();
        }

        public async Task MarkAllReadAsync(Guid userId)
        {
            var unread = await _uow.Notifications.FindAsync(
                n => n.RecipientUserId == userId && !n.IsRead);
            foreach (var n in unread)
            {
                n.IsRead = true;
                n.ReadDate = DateTimeOffset.UtcNow;
                _uow.Notifications.Update(n);
            }
            await _uow.SaveChangesAsync();
        }
    }
}
