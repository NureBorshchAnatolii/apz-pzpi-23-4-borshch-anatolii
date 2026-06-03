namespace CareLink.Domain.Entities
{
    public class CognitiveResult : BaseEntity
    {
        public long Score { get; set; }
        public DateTime CompletedAt { get; set; }
        
        public long ExerciseId { get; set; }
        public CognitiveExercise CognitiveExercise { get; set; } = null!;
        
        public long UserId { get; set; }
        public User User { get; set; } = null!;
    }
}