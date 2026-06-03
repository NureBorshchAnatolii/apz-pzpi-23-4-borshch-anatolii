using CareLink.Application.Contracts.Repositories;
using CareLink.Application.Contracts.Services;
using CareLink.Application.Dtos.Notiffications;
using CareLink.Domain.Entities;

namespace CareLink.Application.Implementations
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepo;

        public NotificationService(INotificationRepository notificationRepo)
        {
            _notificationRepo = notificationRepo;
        }
        
        public async Task<IEnumerable<NotificationDto>> GetUsersNotifications(long userId)
        {
            var query = await _notificationRepo.GetAllAsync();

            var notifications = query
                .Where(n => n.UserId == userId && !n.IsRead)
                .Select(MapToDto)
                .OrderByDescending(n => n.CreatedDate);

            return notifications;
        }

        public async Task MarkAsRead(long userId, long notificationId)
        {
            var notification = await _notificationRepo.GetByIdAsync(notificationId);
            if (notification == null) throw new KeyNotFoundException("Notification not found");

            notification.IsRead = true;
            await _notificationRepo.UpdateAsync(notification);
        }
        
        private NotificationDto MapToDto(Notification notification)
        {
            return new NotificationDto
            {
                Id = notification.Id,
                CreatedDate = notification.CreatedDate,
                Content = notification.Content,
                IsRead = notification.IsRead,
                GroupOfIds = notification.GroupOfIds,
                NotificationTypeId = notification.NotificationTypeId,
            };
        }
    }
}