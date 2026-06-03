using CareLink.Domain.Entities.SubEntities;

namespace CareLink.Application.Contracts.Repositories
{
    public interface IRelationTypeRepository : IGenericRepository<RelationType>, IExistItemRepository<RelationType>
    {
        
    }
}