using CareLink.Application.Contracts.Repositories;
using CareLink.Application.Contracts.Services;
using CareLink.Application.Dtos;
using CareLink.Application.Dtos.IoTDevices;
using CareLink.Domain.Entities;

namespace CareLink.Application.Implementations
{
    public class IoTDeviceService : IIoTDeviceService
    {
        private readonly IIoTDeviceRepository _deviceRepository;
        private readonly IDeviceTypeRepository _deviceTypeRepository;
        private readonly IUserRepository _userRepository;

        public IoTDeviceService(
            IIoTDeviceRepository deviceRepository,
            IDeviceTypeRepository deviceTypeRepository,
            IUserRepository userRepository)
        {
            _deviceRepository = deviceRepository;
            _deviceTypeRepository = deviceTypeRepository;
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<IoTDeviceDto>> GetAllUserDevicesAsync(long userId)
        {
            var devices = await _deviceRepository.GetAllDevicesIncludedAsync();

            return devices
                .Where(d => d.UserId == userId)
                .Select(MapToDto);
        }

        public async Task CreateDeviceAsync(IoTDeviceCreateRequest request)
        {
            await ValidateUser(request.UserId);
            await ValidateDeviceType(request.DeviceTypeId);
            await EnsureSerialNumberNotUsed(request.SerialNumber);

            var device = new IoTDevice
            {
                UserId = request.UserId,
                SerialNumber = request.SerialNumber,
                DeviceTypeId = request.DeviceTypeId
            };

            await _deviceRepository.AddAsync(device);
        }

        public async Task UpdateDeviceAsync(IoTDeviceUpdateRequest request)
        {
            await ValidateUser(request.UserId);
            await ValidateDeviceType(request.DeviceTypeId);

            var device = await _deviceRepository.GetByIdAsync(request.DeviceId)
                         ?? throw new ArgumentException("Device not found");

            EnsureOwnership(request.UserId, device);

            if (device.SerialNumber != request.SerialNumber)
                await EnsureSerialNumberNotUsed(request.SerialNumber);

            device.SerialNumber = request.SerialNumber;
            device.DeviceTypeId = request.DeviceTypeId;

            await _deviceRepository.UpdateAsync(device);
        }
        
        public async Task<IoTDeviceStateDto> GetDeviceStateBySerialNumberIdAsync(string number)
        {
            return await _deviceRepository.GetDeviceStateBySerialNumberAsync(number);
        }

        public async Task ChangeDeviceState(string serialNumber)
        {
            var devices = await _deviceRepository.GetAllDevicesIncludedAsync();
            
            var device = devices.FirstOrDefault(d => d.SerialNumber.Trim() == serialNumber.Trim());
            device.IsActive = !device.IsActive;
            
            await _deviceRepository.UpdateAsync(device);
        }

        private async Task ValidateUser(long userId)
        {
            _ = await _userRepository.GetByIdAsync(userId)
                ?? throw new ArgumentException("User not found");
        }

        private async Task ValidateDeviceType(long typeId)
        {
            var exists = await _deviceTypeRepository.GetByIdAsync(typeId);
            if (exists == null)
                throw new ArgumentException("Device type not found");
        }

        private async Task EnsureSerialNumberNotUsed(string serial)
        {
            var exists = await _deviceRepository.ExistItemAsync(d => d.SerialNumber == serial);
            if (exists)
                throw new InvalidOperationException("Device with this serial number already exists");
        }

        private void EnsureOwnership(long userId, IoTDevice device)
        {
            if (device.UserId != userId)
                throw new UnauthorizedAccessException("User does not own this IoT device");
        }

        private IoTDeviceDto MapToDto(IoTDevice device)
        {
            return new IoTDeviceDto
            {
                Id = device.Id,
                SerialNumber = device.SerialNumber,
                DeviceType = device.DeviceType.Name,
                UserId = device.UserId,
            };
        }
    }

}