using CareLink.Application.Contracts.Repositories;
using CareLink.Domain.Entities;
using CareLink.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;

namespace CareLink.Persistence.Repositories
{
    public class SubscriptionRepository : GenericRepository<Subscription>, ISubscriptionRepository
    {
        public SubscriptionRepository(CareLinkDbContext context) : base(context)
        {
        }
        
        public async Task<Subscription?> GetActiveByUserIdAsync(long userId)
        {
            var now = DateTime.UtcNow;

            return await _context.Subscriptions
                .AsNoTracking()
                .Include(s => s.SubscriptionStatus)
                .FirstOrDefaultAsync(s =>
                    s.UserId == userId &&
                    s.StartedAt <= now &&
                    s.ExpiresAt >= now &&
                    s.SubscriptionStatus.Name == "Active");
        }
    }
}