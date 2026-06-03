using System.ComponentModel.DataAnnotations;

namespace CareLink.Api.Models.Requests
{
    public class RegisterRequest
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name must be less than 50 characters")]
        public string FirstName { get; set; } = null!;

        [Required(ErrorMessage = "Surname is required")]
        [StringLength(100, ErrorMessage = "Surname must be less than 50 characters")]
        public string LastName { get; set; } = null!;

        [Required(ErrorMessage = "Role is required")]
        public long RoleId { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Birth date is required")]
        [DataType(DataType.Date)]
        public DateTime BirthDate { get; set; }
        
        [Required(ErrorMessage = "Address date is required")]
        [StringLength(300, MinimumLength = 5, ErrorMessage = "Address must be at least 6 characters")]
        public string Address { get; set; }
        
        [Required(ErrorMessage = "PhoneNumber date is required")]
        [StringLength(50, MinimumLength = 10, ErrorMessage = "Phone must be at least 10 characters")]
        public string PhoneNumber { get; set; }
    }
}