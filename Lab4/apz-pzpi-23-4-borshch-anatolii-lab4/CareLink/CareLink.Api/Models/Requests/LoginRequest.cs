using System.ComponentModel.DataAnnotations;

namespace CareLink.Api.Models.Requests
{
    public class LoginRequest
    {
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } 

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = null!;
    }
}