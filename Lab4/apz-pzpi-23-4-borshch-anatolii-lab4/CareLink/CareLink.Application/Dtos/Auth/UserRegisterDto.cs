namespace CareLink.Application.Dtos
{
    public record UserRegisterDto(
        string Name,
        string Surname,
        long RoleId,
        string Email,
        string Password,
        DateTime BirthDate,
        string Address,
        string PhoneNumber
    );
}