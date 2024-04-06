using API.Pagination.Shared;

namespace API.Pagination
{
    public class ProductsPriceFilter : QueryStringParameters
    {
        public double? Price { get; set; }
        public string? Option { get; set; }
    }
}
