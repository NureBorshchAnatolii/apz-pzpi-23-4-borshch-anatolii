namespace CareLink.Application.Dtos.Admin
{
    public class AdminUserDto
    {
        public long Id { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public DateTime DateCreated { get; set; }
        public long RoleId { get; set; }
        public string? Role { get; set; }
    }
}
