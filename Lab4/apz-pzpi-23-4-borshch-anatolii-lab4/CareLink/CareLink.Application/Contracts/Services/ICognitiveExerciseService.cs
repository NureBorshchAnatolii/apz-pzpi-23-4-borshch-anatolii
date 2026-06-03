using CareLink.Application.Dtos.CognitiveExercise;

namespace CareLink.Application.Contracts.Services
{
    public interface ICognitiveExerciseService
    {
        Task<IEnumerable<CognitiveExerciseDto>> GetAllCognitiveExercisesAsync();
        Task CreateCognitiveExercise(CognitiveExerciseCreateRequest request);
        Task UpdateCognitiveExercise(CognitiveExerciseUpdateRequest request);
        Task DeleteCognitiveExercise(CognitiveExerciseDeleteRequest request);
        Task ReportResultAsync(CognitiveExerciseResultRequest request);
        Task<IEnumerable<CognitiveExerciseResultDto>> GetUserResultsAsync(long userId);
    }
}