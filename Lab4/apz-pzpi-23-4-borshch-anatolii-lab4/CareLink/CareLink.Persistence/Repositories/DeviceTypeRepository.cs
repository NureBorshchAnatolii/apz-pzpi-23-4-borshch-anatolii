using System.Linq.Expressions;
using CareLink.Application.Contracts.Repositories;
using CareLink.Domain.Entities.SubEntities;
using CareLink.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;

namespace CareLink.Persistence.Repositories
{
    public class DeviceTypeRepository : GenericRepository<DeviceType>, IDeviceTypeRepository
    {
        public DeviceTypeRepository(CareLinkDbContext context) : base(context)
        {
        }
        
        public async Task<bool> ExistItemAsync(Expression<Func<DeviceType, bool>> predicate)
        {
            return await _context.DeviceTypes.AnyAsync(predicate);
        }
    }
}