using CareLink.Domain.Entities.SubEntities;

namespace CareLink.Application.Contracts.Repositories
{
    public interface INotificationTypeRepository : IGenericRepository<NotificationType>, IExistItemRepository<NotificationType>
    {
        
    }
}