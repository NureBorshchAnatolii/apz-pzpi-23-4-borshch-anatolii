namespace CareLink.Api.Models.Requests
{
    public class CognitiveExerciseReport
    {
        public long UserId { get; set; }
        public long Score { get; set; }
        public DateTime CompletedAt { get; set; }
    }
}