using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.DTOs
{
    public class ProductDTO
    {
        public int Id { get; set; }

        [StringLength(100)]
        [Required(ErrorMessage = "The product needs a name")]
        public string? Name { get; set; }

        [StringLength(300)]
        public string? Description { get; set; }
        [Column(TypeName = "Decimal(10, 2)")]
        [Required(ErrorMessage = "The price is required")]
        public double Price { get; set; }

        [Required(ErrorMessage = "The avaible quantity is required")]
        public int AvaibleQuantity { get; set; }
        public int CompanyId { get; set; }
    }
}
