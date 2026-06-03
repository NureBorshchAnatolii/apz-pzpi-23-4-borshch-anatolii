using CareLink.Application.Contracts.Repositories;
using CareLink.Application.Contracts.Services;
using CareLink.Domain.Entities;

namespace CareLink.Application.Implementations
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly ISubscriptionRepository _subscriptionRepo;
        private readonly ISubscriptionPlanTypeRepository _featureRepo;

        public SubscriptionService(
            ISubscriptionRepository subscriptionRepo,
            ISubscriptionPlanTypeRepository featureRepo)
        {
            _subscriptionRepo = subscriptionRepo;
            _featureRepo = featureRepo;
        }

        public async Task<bool> HasAccessAsync(long userId, string featureKey)
        {
            var subscription = await _subscriptionRepo.GetActiveByUserIdAsync(userId);

            if (subscription == null)
                return false;

            if (!IsSubscriptionActive(subscription))
                return false;

            return await _featureRepo.ExistsAsync(
                subscription.SubscriptionTypeId,
                featureKey);
        }

        private static bool IsSubscriptionActive(Subscription s)
        {
            return
                s.StartedAt <= DateTime.UtcNow &&
                s.ExpiresAt >= DateTime.UtcNow &&
                s.SubscriptionStatus.Name == "Active";
        }
    }
}