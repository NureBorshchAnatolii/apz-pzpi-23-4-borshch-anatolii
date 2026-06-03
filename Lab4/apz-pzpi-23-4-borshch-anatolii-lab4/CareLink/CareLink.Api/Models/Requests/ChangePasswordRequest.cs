using System.ComponentModel.DataAnnotations;

namespace CareLink.Api.Models.Requests
{
    public class ChangePasswordRequest
    {
        [Required]
        public string Password { get; set; } = null!;
    }
}