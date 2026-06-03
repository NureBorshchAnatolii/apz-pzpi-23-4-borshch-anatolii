namespace CareLink.Application.Dtos.CognitiveExercise
{
    public record CognitiveExerciseUpdateRequest(
        long Id,
        string Title, 
        string Description,
        long DifficultyId, 
        long TypeId, 
        long UserId
        );
}