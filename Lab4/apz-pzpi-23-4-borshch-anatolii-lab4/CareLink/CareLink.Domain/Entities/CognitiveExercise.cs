using CareLink.Domain.Entities.SubEntities;

namespace CareLink.Domain.Entities
{
    public class CognitiveExercise : BaseEntity
    {
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        
        public long DifficultyId { get; set; }
        public Difficulty Difficulty { get; set; } = null!;
        
        public long TypeId { get; set; }
        public CognitiveExerciseType Type { get; set; } = null!;
        
        public long UserId { get; set; }
        public User User { get; set; } = null!;
        
        public ICollection<CognitiveResult> Results { get; set; } = new List<CognitiveResult>();
    }
}