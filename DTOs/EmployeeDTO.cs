using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class EmployeeDTO
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "The name field id required!")]
        [StringLength(80)]
        public string? Name { get; set; }
        public int CompanyId { get; set; }
    }
}
