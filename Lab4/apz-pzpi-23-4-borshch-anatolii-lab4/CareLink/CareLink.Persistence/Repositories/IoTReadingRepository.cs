using CareLink.Application.Contracts.Repositories;
using CareLink.Domain.Entities;
using CareLink.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;

namespace CareLink.Persistence.Repositories
{
    public class IoTReadingRepository : GenericRepository<IoTReading>,IIoTReadingRepository
    {
        public IoTReadingRepository(CareLinkDbContext context) : base(context)
        {
        }
        
        public async Task<IEnumerable<IoTReading>> GetByUserIdAsync(long userId)
        {
            return await _context.IotDevices
                .Where(d => d.UserId == userId)
                .SelectMany(d => d.Readings)
                .AsNoTracking()
                .OrderByDescending(r => r.ReadDateTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<IoTReading>> GetLatestReadingsAsync(long userId, int count)
        {
            return await _context.IotDevices
                .Where(d => d.UserId == userId)
                .SelectMany(d => d.Readings)
                .AsNoTracking()
                .OrderByDescending(r => r.ReadDateTime)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<IoTReading>> GetReadingsInRangeAsync(long userId, DateTime from, DateTime to)
        {
            return await _context.IotDevices
                .Where(d => d.UserId == userId)
                .SelectMany(d => d.Readings)
                .AsNoTracking()
                .Where(r => r.ReadDateTime >= from && r.ReadDateTime <= to)
                .OrderBy(r => r.ReadDateTime)
                .ToListAsync();
        }
    }
}