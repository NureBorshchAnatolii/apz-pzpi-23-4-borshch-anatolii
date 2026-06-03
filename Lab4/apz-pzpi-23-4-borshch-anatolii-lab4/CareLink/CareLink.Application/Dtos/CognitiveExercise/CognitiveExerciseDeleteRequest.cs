namespace CareLink.Application.Dtos.CognitiveExercise
{
    public record CognitiveExerciseDeleteRequest(
        long UserId,
        long CognitiveExerciseId
        );
}