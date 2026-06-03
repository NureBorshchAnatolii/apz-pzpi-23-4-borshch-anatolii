using CareLink.Application.Contracts.Repositories;
using CareLink.Application.Contracts.Services;
using CareLink.Application.Dtos.User;
using CareLink.Application.Mappers;
using FluentValidation;

namespace CareLink.Application.Implementations
{
    public class UserProfileService : IUserProfileService
    {
        private readonly IUserRepository _userRepository;
        private readonly IValidator<UpdateUserProfileRequest> _updateUserProfileRequestValidator;

        public UserProfileService(IUserRepository userRepository, IValidator<UpdateUserProfileRequest> updateUserProfileRequestValidator)
        {
            _userRepository = userRepository;
            _updateUserProfileRequestValidator = updateUserProfileRequestValidator;
        }

        public async Task<UserProfileDto> GetProfileAsync(long userId)
        {
            var user = await _userRepository.GetByIdWithRoleAsync(userId);

            if (user == null)
                throw new ArgumentException("User not found");
            
            return user.MapToDto();
        }

        public async Task UpdateProfileAsync(long userId, UpdateUserProfileRequest updatedProfile)
        {
            var validationResult = await _updateUserProfileRequestValidator.ValidateAsync(updatedProfile);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new ArgumentException($"Validation failed: {errors}");
            }

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new ArgumentException("User not found");

            user.FirstName = updatedProfile.FirstName;
            user.LastName = updatedProfile.LastName;
            user.Email = updatedProfile.Email;
            user.PhoneNumber = updatedProfile.PhoneNumber;
            user.Address = updatedProfile.Address;
            user.DateOdBirth = updatedProfile.DateOfBirth;

            await _userRepository.UpdateAsync(user);
        }

        public async Task DeleteProfileAsync(long userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
                throw new ArgumentException("User not found");
            
            await _userRepository.DeleteUserWithRelationsAsync(user.Id);
        }
    }
}