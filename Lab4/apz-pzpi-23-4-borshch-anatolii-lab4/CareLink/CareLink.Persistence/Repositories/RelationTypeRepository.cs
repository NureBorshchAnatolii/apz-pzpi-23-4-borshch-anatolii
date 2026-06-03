using System.Linq.Expressions;
using CareLink.Application.Contracts.Repositories;
using CareLink.Domain.Entities.SubEntities;
using CareLink.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;

namespace CareLink.Persistence.Repositories
{
    public class RelationTypeRepository : GenericRepository<RelationType>, IRelationTypeRepository
    {
        public RelationTypeRepository(CareLinkDbContext context) : base(context)
        {
        }

        public async Task<bool> ExistItemAsync(Expression<Func<RelationType, bool>> predicate)
        {
            return await _context.RelationTypes.AnyAsync(predicate);
        }
    }
}