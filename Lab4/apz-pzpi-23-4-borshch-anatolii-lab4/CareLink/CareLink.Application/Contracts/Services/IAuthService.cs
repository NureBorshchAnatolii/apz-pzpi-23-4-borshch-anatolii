using CareLink.Application.Dtos;
using CareLink.Application.Security;

namespace CareLink.Application.Contracts.Services
{
    public interface IAuthService
    {
        public long UserId { get; }
        Task<LoginData> Login(UserLoginDto userLoginData);
        Task Register(UserRegisterDto userData);
        Task Logout();
    }
}