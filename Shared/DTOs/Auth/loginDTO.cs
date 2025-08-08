using System.ComponentModel.DataAnnotations;

namespace homework.Shared.DTOs.Auth
{
    public class loginDTO
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}