using CareLink.Domain.Entities;

namespace CareLink.Application.Contracts.Repositories
{
    public interface ISubscriptionRepository : IGenericRepository<Subscription>
    {
        Task<Subscription?> GetActiveByUserIdAsync(long userId);
    }
}