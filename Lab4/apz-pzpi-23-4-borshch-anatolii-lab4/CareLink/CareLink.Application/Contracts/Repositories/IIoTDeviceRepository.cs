using CareLink.Application.Dtos.IoTDevices;
using CareLink.Domain.Entities;

namespace CareLink.Application.Contracts.Repositories
{
    public interface IIoTDeviceRepository : IGenericRepository<IoTDevice>, IExistItemRepository<IoTDevice>
    {
        Task<IEnumerable<IoTDevice>> GetAllDevicesIncludedAsync();
        Task<IEnumerable<IoTDevice>> GetDevicesByUserIdAsync(long userId);
        Task<IoTDevice?> GetDeviceBySerialNumberAsync(string number);
        Task<IoTDeviceStateDto> GetDeviceStateBySerialNumberAsync(string number);
    }
}