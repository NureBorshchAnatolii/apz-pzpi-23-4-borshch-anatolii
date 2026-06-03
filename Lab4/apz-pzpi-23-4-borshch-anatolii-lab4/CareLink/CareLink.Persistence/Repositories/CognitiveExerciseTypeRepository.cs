using System.Linq.Expressions;
using CareLink.Application.Contracts.Repositories;
using CareLink.Domain.Entities.SubEntities;
using CareLink.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;

namespace CareLink.Persistence.Repositories
{
    public class CognitiveExerciseTypeRepository 
        : GenericRepository<CognitiveExerciseType>, ICognitiveExerciseTypeRepository
    {
        public CognitiveExerciseTypeRepository(CareLinkDbContext context) : base(context)
        {
        }

        public async Task<bool> ExistItemAsync(Expression<Func<CognitiveExerciseType, bool>> predicate)
        {
            return await _context.CognitiveExerciseTypes.AnyAsync(predicate);
        }
    }
}