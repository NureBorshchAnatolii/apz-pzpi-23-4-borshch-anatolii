using CareLink.Application.Contracts.Repositories;
using CareLink.Domain.Entities.SubEntities;
using CareLink.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;

namespace CareLink.Persistence.Repositories
{
    public class SubscriptionPlanTypeRepository : GenericRepository<SubscriptionPlanType>, ISubscriptionPlanTypeRepository
    {
        public SubscriptionPlanTypeRepository(CareLinkDbContext context) : base(context)
        {
        }

        public async Task<bool> ExistsAsync(long planTypeId, string featureKey)
        {
            return await _context.SubscriptionPlanTypes
                .AnyAsync(f =>
                    f.Id == planTypeId &&
                    f.Name == featureKey);
        }
    }
}