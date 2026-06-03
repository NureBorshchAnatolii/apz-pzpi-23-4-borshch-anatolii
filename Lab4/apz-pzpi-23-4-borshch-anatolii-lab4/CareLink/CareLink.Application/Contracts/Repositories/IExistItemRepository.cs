using System.Linq.Expressions;
using CareLink.Domain.Entities;

namespace CareLink.Application.Contracts.Repositories
{
    public interface IExistItemRepository<T> where T : BaseEntity
    {
        Task<bool> ExistItemAsync(Expression<Func<T, bool>> predicate);
    }
}