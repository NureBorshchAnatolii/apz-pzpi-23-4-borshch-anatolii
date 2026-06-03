namespace CareLink.Application.Dtos.CognitiveExercise
{
    public class CognitiveExerciseDto
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Difficulty { get; set; }
        public string Type { get; set; }
        public long UserId { get; set; }
    }
}