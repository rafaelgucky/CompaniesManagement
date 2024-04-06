using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace API.Models
{
    public class Product
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
        public int CompanyId {  get; set; }
        [JsonIgnore]
        public Company? Company { get; set; }

        public Product() { }
        public Product(string name, string description, double price, int avaibleQuantity, int companyId)
        {
            Name = name;
            Description = description;
            Price = price;
            AvaibleQuantity = avaibleQuantity;
            CompanyId = companyId;
        }

        public Product(string name, string description, double price, int avaibleQuantity, Company company)
        {
            Name = name;
            Description = description;
            Price = price;
            AvaibleQuantity = avaibleQuantity;
            CompanyId = company.Id;
        }
    }
}
