namespace CareLink.Application.Dtos.User
{
    public record UpdateUserProfileRequest(
        string FirstName,
        string LastName,
        string Email,
        string PhoneNumber,
        string Address,
        DateTime DateOfBirth);
}