using CareLink.Application.Contracts.Repositories;
using CareLink.Application.Contracts.Services;
using CareLink.Domain.Entities;
using CareLink.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;

namespace CareLink.Persistence.Repositories
{
    public class CognitiveExerciseRepository : GenericRepository<CognitiveExercise>, ICognitiveExerciseRepository
    {
        public CognitiveExerciseRepository(CareLinkDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<CognitiveExercise>> GetAllIncludedAsync()
        {
            return await _context.CognitiveExercises
                .AsNoTracking()
                .Include(x => x.Type)
                .Include(x => x.Difficulty)
                .ToListAsync();
        }
    }
}