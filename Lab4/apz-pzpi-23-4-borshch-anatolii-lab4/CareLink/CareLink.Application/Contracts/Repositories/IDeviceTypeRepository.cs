using CareLink.Domain.Entities.SubEntities;

namespace CareLink.Application.Contracts.Repositories
{
    public interface IDeviceTypeRepository : IGenericRepository<DeviceType>, IExistItemRepository<DeviceType>
    {
        
    }
}