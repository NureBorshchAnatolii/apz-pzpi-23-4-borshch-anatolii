using CareLink.Domain.Entities.SubEntities;

namespace CareLink.Application.Models.Subscription
{
    public class SubscriptionFeature
    {
        public long Id { get; set; }

        public long SubscriptionPlanTypeId { get; set; }
        public SubscriptionPlanType PlanType { get; set; } = null!;

        public string FeatureKey { get; set; } = null!;
    }
}