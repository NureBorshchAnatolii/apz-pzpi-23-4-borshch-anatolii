using CareLink.Domain.Entities.SubEntities;

namespace CareLink.Domain.Entities
{
    public class Relatives : BaseEntity
    {
        public DateTime AddedAt { get; set; }
        
        public long RelationTypeId { get; set; }
        public RelationType RelationType { get; set; } = null!;
        
        public long RelativeUserId { get; set; }
        public User RelativeUser { get; set; } = null!;
        
        public long GuardianUserId { get; set; }
        public User GuardianUser { get; set; } = null!;
    }
}