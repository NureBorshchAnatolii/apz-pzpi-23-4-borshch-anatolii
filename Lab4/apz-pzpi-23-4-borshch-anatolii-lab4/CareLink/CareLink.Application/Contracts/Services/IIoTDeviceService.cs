using CareLink.Application.Dtos;
using CareLink.Application.Dtos.IoTDevices;

namespace CareLink.Application.Contracts.Services
{
    public interface IIoTDeviceService
    {
        Task<IEnumerable<IoTDeviceDto>> GetAllUserDevicesAsync(long userId);
        Task CreateDeviceAsync(IoTDeviceCreateRequest request);
        Task UpdateDeviceAsync(IoTDeviceUpdateRequest request);
        Task<IoTDeviceStateDto> GetDeviceStateBySerialNumberIdAsync(string number);

        Task ChangeDeviceState(string serialNumber);
    }
}