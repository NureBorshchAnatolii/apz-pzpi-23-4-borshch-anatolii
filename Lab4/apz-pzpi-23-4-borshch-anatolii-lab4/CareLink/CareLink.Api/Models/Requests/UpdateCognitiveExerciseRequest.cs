namespace CareLink.Api.Models.Requests
{
    public class UpdateCognitiveExerciseRequest
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public long DifficultyId { get; set; }
        public long TypeId { get; set; }
    }
}