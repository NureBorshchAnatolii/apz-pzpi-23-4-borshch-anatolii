using CareLink.Domain.Entities;
using CareLink.Domain.Entities.SubEntities;

namespace CareLink.Application.Contracts.Repositories
{
    public interface ICognitiveExerciseTypeRepository : IGenericRepository<CognitiveExerciseType>, IExistItemRepository<CognitiveExerciseType>
    {
        
    }
}