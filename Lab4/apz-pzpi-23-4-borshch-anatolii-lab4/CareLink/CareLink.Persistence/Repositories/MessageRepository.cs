using CareLink.Application.Contracts.Repositories;
using CareLink.Domain.Entities;
using CareLink.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;

namespace CareLink.Persistence.Repositories
{
    public class MessageRepository : GenericRepository<Message>, IMessageRepository
    {
        public MessageRepository(CareLinkDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Message>> GetUserMessagesAsync(long userId)
        {
            return await _context.Messages
                .Where(m => m.SenderId == userId || m.ReceiverId == userId)
                .OrderByDescending(m => m.SentAt)
                .ToListAsync();
        }
        
        public async Task<IEnumerable<Message>> GetSentMessagesByUserAsync(long userId)
        {
            return await _context.Messages
                .AsNoTracking()
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Where(m => m.SenderId == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Message>> GetReceivedMessagesByUserAsync(long userId)
        {
            return await _context.Messages
                .AsNoTracking()
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Where(m => m.ReceiverId == userId)
                .ToListAsync();
        }
    }
}