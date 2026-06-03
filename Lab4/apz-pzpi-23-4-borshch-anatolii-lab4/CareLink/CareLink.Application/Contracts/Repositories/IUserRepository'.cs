using CareLink.Domain.Entities;

namespace CareLink.Application.Contracts.Repositories
{
    public interface IUserRepository : IGenericRepository<User>, IExistItemRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);

        Task<User?> GetByIdWithRoleAsync(long id);
        
        Task DeleteUserWithRelationsAsync(long userId);
    }
}