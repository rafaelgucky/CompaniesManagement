using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class CompanyDTO
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "The company name is required!")]
        [StringLength(200)]
        public string? Name { get; set; }
    }
}
