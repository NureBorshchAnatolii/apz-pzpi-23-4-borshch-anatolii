using CareLink.Application.Dtos.User;
using CareLink.Domain.Entities;

namespace CareLink.Application.Mappers
{
    public static class UserMapper
    {
        public static UserProfileDto MapToDto(this User user)
        {
            return new UserProfileDto()
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                DateOfBirth = user.DateOdBirth,
                RoleId = user.RoleId,
                Role = user.Role?.Name
            };
        }
    }
}