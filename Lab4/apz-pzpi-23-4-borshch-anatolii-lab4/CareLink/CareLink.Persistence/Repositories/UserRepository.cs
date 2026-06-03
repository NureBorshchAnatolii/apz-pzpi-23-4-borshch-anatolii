using System.Linq.Expressions;
using CareLink.Application.Contracts.Repositories;
using CareLink.Domain.Entities;
using CareLink.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;

namespace CareLink.Persistence.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(CareLinkDbContext context) : base(context)
        {
        }
        
        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users.Include(x => x.Role)
                .FirstOrDefaultAsync(x => x.Email == email);
        }

        public async Task<User?> GetByIdWithRoleAsync(long id)
        {
            return await _context.Users.Include(x => x.Role)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<bool> ExistItemAsync(Expression<Func<User, bool>> predicate)
        {
            return await _context.Users.AnyAsync(predicate);
        }
        
        public async Task DeleteUserWithRelationsAsync(long userId)
        {
            var user = await _context.Users
                .Include(u => u.SentMessages)
                .Include(u => u.ReceivedMessages)
                .Include(u => u.IoTDevices)
                .Include(u => u.Notifications)
                .Include(u => u.Guardians)
                .Include(u => u.RelativeOf)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                throw new ArgumentException("User not found");

            _context.Messages.RemoveRange(user.SentMessages);
            _context.Messages.RemoveRange(user.ReceivedMessages);

            _context.IotDevices.RemoveRange(user.IoTDevices);
            _context.Notifications.RemoveRange(user.Notifications);

            foreach (var guardian in user.Guardians)
                _context.Relatives.Remove(guardian);

            foreach (var relative in user.RelativeOf)
                _context.Relatives.Remove(relative);

            var subscriptions = _context.Set<Subscription>().Where(s => s.UserId == userId);
            _context.Set<Subscription>().RemoveRange(subscriptions);

            _context.Users.Remove(user);

            await _context.SaveChangesAsync();
        }
    }
}