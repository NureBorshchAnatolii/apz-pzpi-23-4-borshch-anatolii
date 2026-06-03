using CareLink.Application.Dtos.Messages;
using CareLink.Domain.Entities;

namespace CareLink.Application.Contracts.Services
{
    public interface IMessageService
    {
        Task<IEnumerable<MessageDto>> GetUserMessagesAsync(long userId, long reciverId);
        Task<MessageDto> CreateMessageAsync(MessageCreateRequest request);
        Task<MessageDto> UpdateMessageAsync(MessageUpdateRequest request);
        Task DeleteMessageAsync(MessageDeleteRequest request);
    }
}