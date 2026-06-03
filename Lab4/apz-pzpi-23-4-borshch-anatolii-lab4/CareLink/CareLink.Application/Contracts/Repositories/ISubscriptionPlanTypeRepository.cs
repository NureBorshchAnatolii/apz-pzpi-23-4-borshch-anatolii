using CareLink.Domain.Entities.SubEntities;

namespace CareLink.Application.Contracts.Repositories
{
    public interface ISubscriptionPlanTypeRepository : IGenericRepository<SubscriptionPlanType>
    {
        Task<bool> ExistsAsync(long subscriptionPlanTypeId, string featureKey);
    }
}