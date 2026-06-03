using CareLink.Application.Dtos.Notiffications;

namespace CareLink.Application.Contracts.Services
{
    public interface INotificationService
    {
        Task<IEnumerable<NotificationDto>> GetUsersNotifications(long userId);
        Task MarkAsRead(long userId, long notificationId);
    }
}