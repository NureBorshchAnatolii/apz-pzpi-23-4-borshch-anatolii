using CareLink.Domain.Entities;

namespace CareLink.Application.Contracts.Repositories
{
    public interface ICognitiveExerciseRepository : IGenericRepository<CognitiveExercise>
    {
        Task<IEnumerable<CognitiveExercise>> GetAllIncludedAsync();
    }
}