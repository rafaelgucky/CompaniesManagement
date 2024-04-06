using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class LoginModelDTO
    {
        [Required(ErrorMessage = "The name is required")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "The password is required")]
        public string? Password { get; set; }
    }
}
