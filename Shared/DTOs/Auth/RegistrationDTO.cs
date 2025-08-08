using System.ComponentModel.DataAnnotations;

namespace homework.DTOs.Auth
{
    public class RegisterRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string ConfirmPassword { get; set; }

        public string Role { get; set; } = "User"; 
    }
}