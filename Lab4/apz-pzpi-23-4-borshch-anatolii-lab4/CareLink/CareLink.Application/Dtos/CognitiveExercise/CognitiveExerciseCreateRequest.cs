namespace CareLink.Application.Dtos.CognitiveExercise
{
    public record CognitiveExerciseCreateRequest(
        string Title, 
        string Description,
        long DifficultyId, 
        long TypeId, 
        long UserId
        );
}