using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace API.Models
{
    public class Employee
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "The name field id required!")]
        [StringLength(80)]
        public string? Name { get; set; }

        [Column(TypeName = "decimal(3, 2)")]
        public double Hours { get; set; }

        [StringLength(300)]
        public string? ImageURL {  get; set; }

        public int CompanyId { get; set; }
        [JsonIgnore]
        public Company? Company { get; set; }
    }
}
