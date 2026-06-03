using CareLink.Application.Contracts.Repositories;
using CareLink.Application.Contracts.Services;
using CareLink.Application.Dtos.Messages;
using CareLink.Domain.Entities;

namespace CareLink.Application.Notifications
{
    public sealed class MessageNotificationDecorator : IMessageService
    {
        private readonly IMessageService _inner;
        private readonly INotificationRepository _notificationRepository;
        private readonly INotificationContentFactory _contentFactory;

        public MessageNotificationDecorator(
            IMessageService inner,
            INotificationRepository notificationRepository,
            INotificationContentFactory contentFactory)
        {
            _inner = inner;
            _notificationRepository = notificationRepository;
            _contentFactory = contentFactory;
        }

        public Task<IEnumerable<MessageDto>> GetUserMessagesAsync(long userId, long receiverId)
            => _inner.GetUserMessagesAsync(userId, receiverId);

        public async Task<MessageDto> CreateMessageAsync(MessageCreateRequest request)
        {
            var result = await _inner.CreateMessageAsync(request);

            var notification = new Notification
            {
                CreatedDate = DateTime.UtcNow,
                IsRead = false,
                UserId = request.ReceiverId,
                NotificationTypeId = 1,
                Content = _contentFactory.Build(1, request),
                GroupOfIds = _contentFactory.BuildGroup(request)
            };

            await _notificationRepository.AddAsync(notification);

            return result;
        }

        public async Task<MessageDto> UpdateMessageAsync(MessageUpdateRequest request)
            => await _inner.UpdateMessageAsync(request);

        public Task DeleteMessageAsync(MessageDeleteRequest request)
            => _inner.DeleteMessageAsync(request);
    }
}