using CareLink.Application.Contracts.Repositories;
using CareLink.Application.Contracts.Services;
using CareLink.Application.Dtos.Admin;

namespace CareLink.Application.Implementations
{
    public class AdminService : IAdminService
    {
        private readonly IIoTDeviceRepository _deviceRepo;
        private readonly FileLogReaderService _logReaderService;
        private readonly INotificationRepository _notificationRepo;
        private readonly IRoleRepository _roleRepo;
        private readonly ISubscriptionRepository _subscriptionRepo;
        private readonly IUserProfileService _userProfileService;
        private readonly IUserRepository _userRepo;

        public AdminService(
            IUserRepository userRepo,
            IRoleRepository roleRepo,
            IIoTDeviceRepository deviceRepo,
            ISubscriptionRepository subscriptionRepo,
            INotificationRepository notificationRepo,
            IUserProfileService userProfileService,
            FileLogReaderService logReaderService)
        {
            _userRepo = userRepo;
            _roleRepo = roleRepo;
            _deviceRepo = deviceRepo;
            _subscriptionRepo = subscriptionRepo;
            _notificationRepo = notificationRepo;
            _userProfileService = userProfileService;
            _logReaderService = logReaderService;
        }

        public async Task<SystemStateDto> GetSystemStateAsync()
        {
            var totalUsers = await _userRepo.GetAllAsync();
            var iotDevices = await _deviceRepo.GetAllAsync();
            var activeSubscriptions = await _subscriptionRepo.GetAllAsync();
            var notifications = await _notificationRepo.GetAllAsync();

            return new SystemStateDto
            {
                TotalUsers = totalUsers.Count,
                IoTDevices = iotDevices.Count,
                ActiveSubscriptions = activeSubscriptions.Count,
                NotificationsLast24h = notifications.Count
            };
        }

        public async Task ChangeUserRoleAsync(long userId, long roleId)
        {
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null)
                throw new ArgumentException("User not found");

            var role = await _roleRepo.GetByIdAsync(roleId);
            if (role == null)
                throw new ArgumentException("Role not found");

            user.RoleId = roleId;
            await _userRepo.UpdateAsync(user);
        }

        public async Task<IEnumerable<AdminUserDto>> GetAllUsersAsync()
        {
            var users = await _userRepo.GetAllAsync();
            var roles = (await _roleRepo.GetAllAsync()).ToDictionary(r => r.Id, r => r.Name);

            return users.Select(u => new AdminUserDto
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                DateCreated = u.DateCreated,
                RoleId = u.RoleId,
                Role = roles.TryGetValue(u.RoleId, out var name) ? name : null,
            }).ToList();
        }

        public async Task DeleteUserAsync(long userId)
        {
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null)
                throw new ArgumentException("User not found");

            await _userProfileService.DeleteProfileAsync(userId);
        }

        public async Task<IEnumerable<SystemLogDto>> GetLogsAsync(
            DateTime? from = null,
            DateTime? to = null,
            string? level = null)
        {
            var logs = await _logReaderService.ReadLogsAsync(from, to, level);
            return logs;
        }
    }
}