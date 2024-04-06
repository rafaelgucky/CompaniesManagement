using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace API.Models
{
    public class Company
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "The company name is required!")]
        [StringLength(200)]
        public string? Name { get; set; }
        [JsonIgnore]
        public ICollection<Employee>? Employees { get; set; }
        [JsonIgnore]
        public ICollection<Product>? Products { get; set; }
    }
}
