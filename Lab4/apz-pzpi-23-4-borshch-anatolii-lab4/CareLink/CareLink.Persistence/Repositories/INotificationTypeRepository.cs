using System.Linq.Expressions;
using CareLink.Application.Contracts.Repositories;
using CareLink.Domain.Entities.SubEntities;
using CareLink.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;

namespace CareLink.Persistence.Repositories
{
    public class NotificationTypeRepository : GenericRepository<NotificationType>, INotificationTypeRepository
    {
        public NotificationTypeRepository(CareLinkDbContext context) : base(context)
        {
        }

        public async Task<bool> ExistItemAsync(Expression<Func<NotificationType, bool>> predicate)
        {
            return await _context.NotificationTypes.AnyAsync(predicate);
        }
    }
}