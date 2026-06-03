using CareLink.Application.Dtos.User;

namespace CareLink.Application.Contracts.Services
{
    public interface IUserProfileService
    {
        Task<UserProfileDto> GetProfileAsync(long userId);
        Task UpdateProfileAsync(long userId, UpdateUserProfileRequest updatedProfile);
        Task DeleteProfileAsync(long userId);
    }
}