using System.Linq.Expressions;
using CareLink.Application.Contracts.Repositories;
using CareLink.Domain.Entities.SubEntities;
using CareLink.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;

namespace CareLink.Persistence.Repositories
{
    public class RoleRepository : GenericRepository<Role>, IRoleRepository
    {
        public RoleRepository(CareLinkDbContext context) : base(context)
        {
        }

        public async Task<long> IsRoleValid(string role)
        {
            var currentRole = await _context.Roles.FirstOrDefaultAsync(x => x.Name == role);
            
            if(currentRole == null || role == "Admin")
                throw new ArgumentException("Role does not exist");
            
            return currentRole.Id;
        }

        public async Task<long> IsRoleValid(long roleId)
        {
            var currentRole = await _context.Roles.FirstOrDefaultAsync(x => x.Id == roleId);
            
            if(currentRole == null || roleId == 1 || roleId == 2)
                throw new ArgumentException("Role does not exist");
            
            return currentRole.Id;
        }

        public async Task<bool> ExistItemAsync(Expression<Func<Role, bool>> predicate)
        {
            return await _context.Roles.AnyAsync(predicate);
        }
    }
}