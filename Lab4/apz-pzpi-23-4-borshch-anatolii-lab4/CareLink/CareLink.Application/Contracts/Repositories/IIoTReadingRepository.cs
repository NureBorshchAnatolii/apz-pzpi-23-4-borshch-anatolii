using CareLink.Domain.Entities;

namespace CareLink.Application.Contracts.Repositories
{
    public interface IIoTReadingRepository : IGenericRepository<IoTReading>
    {
        Task<IEnumerable<IoTReading>> GetByUserIdAsync(long userId);
        Task<IEnumerable<IoTReading>> GetLatestReadingsAsync(long userId, int count);
        Task<IEnumerable<IoTReading>> GetReadingsInRangeAsync(long userId, DateTime from, DateTime to);
    }
}