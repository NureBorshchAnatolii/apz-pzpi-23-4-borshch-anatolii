namespace CareLink.Application.Dtos.CognitiveExercise
{
    public class CognitiveExerciseResultDto
    {
        public long Id { get; set; }
        public long Score { get; set; }
        public DateTime CompletedAt { get; set; }
        public long ExerciseId { get; set; }
        public long UserId { get; set; }
    }
}