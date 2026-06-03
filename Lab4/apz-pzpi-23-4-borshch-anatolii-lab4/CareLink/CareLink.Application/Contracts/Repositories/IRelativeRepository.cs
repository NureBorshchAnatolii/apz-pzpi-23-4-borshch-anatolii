using CareLink.Domain.Entities;

namespace CareLink.Application.Contracts.Repositories
{
    public interface IRelativeRepository : IGenericRepository<Relatives>, IExistItemRepository<Relatives>
    {
        Task<IEnumerable<Relatives>> GetAllIncludedRelatives();
    }
}