using CareLink.Domain.Entities;

namespace CareLink.Application.Contracts.Security
{
    public interface IJwtProvider
    {
        string GenerateToken(User user);
    }
}