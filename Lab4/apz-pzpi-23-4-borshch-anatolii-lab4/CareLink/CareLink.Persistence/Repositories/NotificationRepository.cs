using CareLink.Application.Contracts.Repositories;
using CareLink.Domain.Entities;
using CareLink.Domain.Entities.SubEntities;
using CareLink.Persistence.DbContext;

namespace CareLink.Persistence.Repositories
{
    public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
    {
        public NotificationRepository(CareLinkDbContext context) : base(context)
        {
        }
    }
}