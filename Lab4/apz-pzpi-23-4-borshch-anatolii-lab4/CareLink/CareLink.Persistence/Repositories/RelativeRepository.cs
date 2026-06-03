using System.Linq.Expressions;
using CareLink.Application.Contracts.Repositories;
using CareLink.Domain.Entities;
using CareLink.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;

namespace CareLink.Persistence.Repositories
{
    public class RelativeRepository : GenericRepository<Relatives>, IRelativeRepository
    {
        public RelativeRepository(CareLinkDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Relatives>> GetAllIncludedRelatives()
        {
            return await _context.Relatives
                .AsNoTracking()
                .Include(x => x.RelationType)
                .Include(x => x.GuardianUser)
                .Include(x => x.RelativeUser)
                .ToListAsync();
        }
        
        public async Task<bool> ExistItemAsync(Expression<Func<Relatives, bool>> predicate)
        {
            return await _context.Relatives.AnyAsync(predicate);
        }
    }
}