using System.Security.Claims;
using CareLink.Application.Contracts.Security;
using Microsoft.AspNetCore.Http;

namespace CareLink.Security.Security
{
    public class UserContext : IUserContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserContext(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public long GetApplicationUserId()
        {
            var claim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);

            if (claim == null || !long.TryParse(claim.Value, out var id))
            {
                throw new UnauthorizedAccessException("User is not authenticated");
            }

            return id;
        }
    }
}