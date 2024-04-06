using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class RegisterModelDTO
    {
        [Required(ErrorMessage = "The name is required")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "The password is required")]
        public string? Password { get; set; }

        [EmailAddress]
        [Required(ErrorMessage = "The Email is required")]
        public string? Email { get; set; }
    }
}
