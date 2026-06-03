using CareLink.Application.Contracts.Repositories;
using CareLink.Domain.Entities;
using CareLink.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;

namespace CareLink.Persistence.Repositories
{
    public class CognitiveResultRepository : GenericRepository<CognitiveResult>, ICognitiveResultRepository
    {
        public CognitiveResultRepository(CareLinkDbContext context) : base(context) { }

        public async Task<IEnumerable<CognitiveResult>> GetByUserIdAsync(long userId)
        {
            return await _context.CognitiveResults
                .Include(r => r.CognitiveExercise)
                .Where(r => r.UserId == userId)
                .ToListAsync();
        }
    }
}