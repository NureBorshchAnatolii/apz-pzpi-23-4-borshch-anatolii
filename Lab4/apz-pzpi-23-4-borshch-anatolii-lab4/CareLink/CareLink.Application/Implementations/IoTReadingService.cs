using CareLink.Application.Contracts.Repositories;
using CareLink.Application.Contracts.Services;
using CareLink.Application.Dtos.IoTDevices;
using CareLink.Domain.Entities;

namespace CareLink.Application.Implementations
{
    public class IoTReadingService : IIoTReadingService
    {
        private readonly IIoTDeviceRepository _deviceRepository;
        private readonly IIoTReadingRepository _readingRepository;

        public IoTReadingService(
            IIoTReadingRepository readingRepository,
            IIoTDeviceRepository deviceRepository)
        {
            _readingRepository = readingRepository;
            _deviceRepository = deviceRepository;
        }

        public async Task<IoTReadingDto> CreateReadingAsync(IoTReadingCreateRequest request)
        {
            var device = await _deviceRepository.GetDeviceBySerialNumberAsync(request.SerialNumber);
            if (device == null)
                throw new ArgumentException($"Device with serial number {request.SerialNumber} does not exist");
            
            var reading = new IoTReading
            {
                ReadDateTime = request.ReadDateTime,
                Pulse = request.Pulse,
                ActivityLevel = request.ActivityLevel,
                Temperature = request.Temperature,
                DeviceId = device.Id
            };

            await _readingRepository.AddAsync(reading);

            return MapToDto(reading);
        }

        public async Task<IEnumerable<IoTReadingDto>> GetAllReadingsByUserAsync(long userId)
        {
            var readings = await _readingRepository.GetByUserIdAsync(userId);
            return readings.Select(MapToDto);
        }

        public async Task<IEnumerable<IoTReadingDto>> GetLatestReadingsByUserAsync(long userId, int count)
        {
            var readings = await _readingRepository.GetLatestReadingsAsync(userId, count);
            return readings.Select(MapToDto);
        }

        public async Task<IEnumerable<IoTReadingDto>> GetReadingsInRangeByUserAsync(long userId, DateTime from,
            DateTime to)
        {
            var readings = await _readingRepository.GetReadingsInRangeAsync(userId, from, to);
            return readings.Select(MapToDto);
        }

        private async Task EnsureDeviceOwnedByUser(long deviceId, long userId)
        {
            var device = await _deviceRepository.GetByIdAsync(deviceId);

            if (device == null)
                throw new Exception("Device not found.");

            if (device.UserId != userId)
                throw new UnauthorizedAccessException("User does not own this device.");
        }

        private IoTReadingDto MapToDto(IoTReading reading)
        {
            return new IoTReadingDto
            {
                Id = reading.Id,
                ReadDateTime = reading.ReadDateTime,
                Pulse = reading.Pulse,
                ActivityLevel = reading.ActivityLevel,
                Temperature = reading.Temperature
            };
        }
    }
}