using CareLink.Domain.Entities.SubEntities;

namespace CareLink.Application.Contracts.Repositories
{
    public interface IDifficultyRepository : IGenericRepository<Difficulty>, IExistItemRepository<Difficulty>
    {
        
    }
}