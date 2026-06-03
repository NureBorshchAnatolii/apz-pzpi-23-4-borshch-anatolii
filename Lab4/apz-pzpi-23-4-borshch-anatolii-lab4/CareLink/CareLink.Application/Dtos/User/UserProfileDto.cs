namespace CareLink.Application.Dtos.User
{
    public class UserProfileDto
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string Address { get; set; } = null!;
        public DateTime DateOfBirth { get; set; }
        public long RoleId { get; set; }
        public string? Role { get; set; }
    }
}