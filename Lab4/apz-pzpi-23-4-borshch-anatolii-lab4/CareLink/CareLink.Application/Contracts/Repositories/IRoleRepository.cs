using CareLink.Domain.Entities.SubEntities;

namespace CareLink.Application.Contracts.Repositories
{
    public interface IRoleRepository : IGenericRepository<Role>, IExistItemRepository<Role>
    {
        Task<long> IsRoleValid(string role);
        Task<long> IsRoleValid(long role);
    }
}