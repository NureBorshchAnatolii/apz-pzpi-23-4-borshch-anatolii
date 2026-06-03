using CareLink.Application.Contracts.Repositories;
using CareLink.Application.Contracts.Services;
using CareLink.Application.Dtos.Messages;
using CareLink.Application.Notifications;
using CareLink.Domain.Entities;

namespace CareLink.Application.Implementations
{
    public class MessageService : IMessageService
    {
        private readonly IMessageRepository _messageRepository;
        private readonly IUserRepository _userRepository;

        public MessageService(
            IMessageRepository messageRepository,
            IUserRepository userRepository)
        {
            _messageRepository = messageRepository;
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<MessageDto>> GetUserMessagesAsync(long userId, long receiverId)
        {
            var userExists = await _userRepository.GetByIdAsync(userId);
            if (userExists is null)
                throw new UnauthorizedAccessException("User not found");

            var messages = await _messageRepository.GetAllAsync();
            var usersMessages = messages
                .Where(m =>
                    (m.SenderId == userId && m.ReceiverId == receiverId) ||
                    (m.SenderId == receiverId && m.ReceiverId == userId))
                .OrderBy(m => m.SentAt);

            return usersMessages.Select(MapToDto);
        }

        public async Task<MessageDto> CreateMessageAsync(MessageCreateRequest request)
        {
            var sender = await _userRepository.GetByIdAsync(request.SenderId);
            if (sender is null)
                throw new ArgumentException("Sender does not exist");

            var receiver = await _userRepository.GetByIdAsync(request.ReceiverId);
            if (receiver is null)
                throw new ArgumentException("Receiver does not exist");

            var message = new Message
            {
                Content = request.Content,
                SenderId = request.SenderId,
                ReceiverId = request.ReceiverId,
                SentAt = DateTime.UtcNow
            };

            await _messageRepository.AddAsync(message);

            return MapToDto(message);
        }

        public async Task<MessageDto> UpdateMessageAsync(MessageUpdateRequest request)
        {
            var message = await _messageRepository.GetByIdAsync(request.MessageId);
            if (message is null)
                throw new ArgumentException("Message not found");

            message.Content = request.Content;

            await _messageRepository.UpdateAsync(message);

            return MapToDto(message);
        }

        public async Task DeleteMessageAsync(MessageDeleteRequest request)
        {
            var message = await _messageRepository.GetByIdAsync(request.MessageId);

            if (message == null)
                throw new Exception("Message not found");

            if (message.SenderId != request.UserId)
                throw new UnauthorizedAccessException("You cannot delete this message");

            await _messageRepository.DeleteAsync(message);
        }
        
        private MessageDto MapToDto(Message message)
        {
            return new MessageDto
            {
                Id = message.Id,
                Content = message.Content,
                SentAt = message.SentAt,
                SenderId = message.SenderId,
                ReceiverId = message.ReceiverId
            };
        }
    }
}