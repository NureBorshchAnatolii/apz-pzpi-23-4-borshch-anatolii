namespace CareLink.Application.Dtos.CognitiveExercise
{
    public record CognitiveExerciseResultRequest(
        long ExerciseId, 
        long Score,
        DateTime CompletedAt, 
        long UserId
        );
}