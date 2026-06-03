using CareLink.Domain.Entities;

namespace CareLink.Application.Contracts.Security
{
    public interface IPasswordHasher<TUser> where TUser : class
    {
        string HashPassword(TUser user, string password);
        bool VerifyHashedPassword(TUser user, string hashedPassword, string providedPassword);
    }
}