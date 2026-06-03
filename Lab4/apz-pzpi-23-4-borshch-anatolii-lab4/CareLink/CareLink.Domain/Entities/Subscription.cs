using CareLink.Domain.Entities.SubEntities;

namespace CareLink.Domain.Entities
{
    public class Subscription : BaseEntity
    {
        public string StripeSubscriptionId { get; set; } = null!;
        public DateTime StartedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        
        public long UserId { get; set; }
        public User User { get; set; } = null!;
        
        public long SubscriptionStatusId { get; set; }
        public SubscriptionStatus SubscriptionStatus { get; set; } = null!;
        
        public long SubscriptionTypeId { get; set; }
        public SubscriptionPlanType SubscriptionType { get; set; } = null!;
    }
}