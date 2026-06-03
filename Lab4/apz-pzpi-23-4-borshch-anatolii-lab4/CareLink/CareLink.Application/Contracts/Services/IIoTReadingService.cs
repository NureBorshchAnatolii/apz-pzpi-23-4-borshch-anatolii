using CareLink.Application.Dtos.IoTDevices;

namespace CareLink.Application.Contracts.Services
{
    public interface IIoTReadingService
    {
        Task<IoTReadingDto> CreateReadingAsync(IoTReadingCreateRequest request);
        Task<IEnumerable<IoTReadingDto>> GetAllReadingsByUserAsync(long userId);
        Task<IEnumerable<IoTReadingDto>> GetLatestReadingsByUserAsync(long userId, int count);
        Task<IEnumerable<IoTReadingDto>> GetReadingsInRangeByUserAsync(long userId, DateTime from, DateTime to);
    }
}