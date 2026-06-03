using CareLink.Domain.Entities.SubEntities;

namespace CareLink.Domain.Entities
{
    public class Notification : BaseEntity
    {
        public DateTime CreatedDate { get; set; }
        public string Content { get; set; } = null!;
        public bool IsRead { get; set; }
        public string GroupOfIds { get; set; } = null!;
        
        public long UserId { get; set; }
        public User User { get; set; } = null!;
        
        public long NotificationTypeId { get; set; }
        public NotificationType Type { get; set; } = null!;
    }
}