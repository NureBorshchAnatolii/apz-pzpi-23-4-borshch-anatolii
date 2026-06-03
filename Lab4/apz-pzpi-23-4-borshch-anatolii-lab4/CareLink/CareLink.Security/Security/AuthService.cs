using System.Security.Claims;
using CareLink.Application.Contracts.Repositories;
using CareLink.Application.Contracts.Security;
using CareLink.Application.Contracts.Services;
using CareLink.Application.Dtos;
using CareLink.Application.Security;
using CareLink.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace CareLink.Security.Security
{
    public class AuthService: IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtProvider _jwtProvider;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRoleRepository _roleRepository;
        private readonly ILogger<AuthService> _logger;

        public AuthService(IUserRepository userRepository, IJwtProvider jwtProvider
            , IPasswordHasher<User> passwordHasher, IHttpContextAccessor httpContextAccessor
            , IRoleRepository roleRepository, ILogger<AuthService> logger)
        {
            _userRepository = userRepository;
            _jwtProvider = jwtProvider;
            _passwordHasher = passwordHasher;
            _httpContextAccessor = httpContextAccessor;
            _roleRepository = roleRepository;
            _logger = logger;
        }

        public long UserId 
        {
            get
            {
                var userIdClaim = _httpContextAccessor.HttpContext?
                    .User?
                    .FindFirst(ClaimTypes.NameIdentifier)?
                    .Value;

                return long.TryParse(userIdClaim, out var id) ? id : 0;
            }
        }
        
        public async Task<LoginData> Login(UserLoginDto userLoginDto)
        {
            User? user = await _userRepository.GetByEmailAsync(userLoginDto.Email);
            
            if (user == null)
            {
                throw new UnauthorizedAccessException("Invalid credentials.");
            }

            var validPassword = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, userLoginDto.Password);
            
            if (!validPassword)
            {
                throw new UnauthorizedAccessException("Invalid credentials.");
            }

            var token = _jwtProvider.GenerateToken(user);
            
            return new LoginData
            {
                UserId = user.Id,
                Token = token,
            };
        }

        public async Task Register(UserRegisterDto userData)
        {
            var users = await _userRepository.GetAllAsync();
            
            var existingUser = users.FirstOrDefault(u => u.Email == userData.Email);
            if (existingUser != null)
            {
                throw new InvalidOperationException("User already exists.");
            }
            
            var roleNumber = await _roleRepository.IsRoleValid(userData.RoleId);
            
            var user = new User
            {
                FirstName = userData.Name,
                LastName = userData.Surname,
                RoleId = roleNumber,
                Email = userData.Email,
                DateOdBirth = userData.BirthDate,
                DateCreated = DateTime.UtcNow,
                Address = userData.Address,
                PhoneNumber = userData.PhoneNumber,
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, userData.Password);

            _logger.LogWarning(
                "[DEV] Registered user {Email} with password '{Password}' -> hash {Hash}",
                user.Email, userData.Password, user.PasswordHash);

            await _userRepository.AddAsync(user);
        }

        public async Task Logout()
        {
            await Task.CompletedTask;
        }
    }
}