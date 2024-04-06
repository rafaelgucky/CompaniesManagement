using API.Models;
using API.Pagination;
using API.Pagination.Shared;
using X.PagedList;

namespace API.Repositories.Interfaces
{
    public interface IProductRepository : IRepository<Product>
    {
        Task<IPagedList<Product>> PaginationGetAsync(ProductsParameters parameters);
        Task<IPagedList<Product>> PriceFilterAsync(ProductsPriceFilter filter);
    }
}
