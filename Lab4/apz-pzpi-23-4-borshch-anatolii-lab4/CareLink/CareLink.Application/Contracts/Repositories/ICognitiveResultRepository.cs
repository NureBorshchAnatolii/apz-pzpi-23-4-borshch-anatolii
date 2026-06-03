using CareLink.Domain.Entities;

namespace CareLink.Application.Contracts.Repositories
{
    public interface ICognitiveResultRepository : IGenericRepository<CognitiveResult>
    {
        Task<IEnumerable<CognitiveResult>> GetByUserIdAsync(long userId);
    }
}