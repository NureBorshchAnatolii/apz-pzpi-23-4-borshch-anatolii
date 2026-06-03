using System.Linq.Expressions;
using CareLink.Application.Contracts.Repositories;
using CareLink.Application.Dtos.IoTDevices;
using CareLink.Domain.Entities;
using CareLink.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;

namespace CareLink.Persistence.Repositories
{
    public class IoTDeviceRepository : GenericRepository<IoTDevice>, IIoTDeviceRepository
    {
        public IoTDeviceRepository(CareLinkDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<IoTDevice>> GetAllDevicesIncludedAsync()
        {
            return await _context.IotDevices
                .Include(x => x.DeviceType)
                .Include(x => x.User)
                .ToListAsync();
        }

        public async Task<bool> ExistItemAsync(Expression<Func<IoTDevice, bool>> predicate)
        {
            return await _context.IotDevices.AnyAsync(predicate);
        }
        
        public async Task<IEnumerable<IoTDevice>> GetDevicesByUserIdAsync(long userId)
        {
            return await _context.IotDevices
                .AsNoTracking()
                .Include(d => d.DeviceType)
                .Include(d => d.Readings)
                .Where(d => d.UserId == userId)
                .ToListAsync();
        }

        public async Task<IoTDevice?> GetDeviceBySerialNumberAsync(string number)
        {
            return await _context.IotDevices.FirstOrDefaultAsync(d => d.SerialNumber.Trim() == number.Trim());
        }

        public async Task<IoTDeviceStateDto> GetDeviceStateBySerialNumberAsync(string number)
        {
            var device = await _context.IotDevices.FirstOrDefaultAsync(d => d.SerialNumber.Trim() == number.Trim());
            return new IoTDeviceStateDto() { IsActive = device.IsActive };
        }
    }
}