using API.Data;
using API.Models;
using API.Pagination;
using API.Repositories.Interfaces;
using X.PagedList;

namespace API.Repositories
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        public ProductRepository(AplicationContext context) : base(context) { }

        public async Task<IPagedList<Product>> PaginationGetAsync(ProductsParameters parameters)
        {
            var allProducts = await GetAsync();
            var products = allProducts.OrderBy(product => product.Id).AsQueryable();
            //return PagedList<Product>.ToPagedList(products, parameters.PageNumber, parameters.PageSize);
            return await products.ToPagedListAsync(parameters.PageNumber, parameters.PageSize);
        }

        public async Task<IPagedList<Product>> PriceFilterAsync(ProductsPriceFilter filter)
        {
            var products = await GetAsync();

            if (filter.Price.HasValue && filter.Option != null)
            {
                switch (filter.Option.ToLower())
                {
                    case "maior":
                        products = products.Where(p => p.Price > filter.Price).OrderBy(p => p.Price).ToList();
                        break;
                    case "menor":
                        products = products.Where(p => p.Price < filter.Price).OrderBy(p => p.Price).ToList();
                        break;
                    case "igual":
                        products = products.Where(p => p.Price == filter.Price).OrderBy(p => p.Price).ToList();
                        break;
                }
            }

            //return PagedList<Product>.ToPagedList(products.AsQueryable(), filter.PageNumber, filter.PageSize);
            return await products.ToPagedListAsync(filter.PageNumber, filter.PageSize);
        }
    }
}
