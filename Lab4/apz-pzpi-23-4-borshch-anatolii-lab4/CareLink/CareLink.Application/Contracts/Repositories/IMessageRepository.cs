using CareLink.Domain.Entities;

namespace CareLink.Application.Contracts.Repositories
{
    public interface IMessageRepository : IGenericRepository<Message>
    {
        Task<IEnumerable<Message>> GetUserMessagesAsync(long userId);
        Task<IEnumerable<Message>> GetSentMessagesByUserAsync(long userId);
        Task<IEnumerable<Message>> GetReceivedMessagesByUserAsync(long userId);
    }
}