using API.Models;
using API.Pagination;
using API.Pagination.Shared;
using X.PagedList;

namespace API.Repositories.Interfaces
{
    public interface ICompanyRepository : IRepository<Company>
    {
        Task<IPagedList<Company>> PaginationGetAsync(CompanyParameters parameters);
        Company? GetEmployees(int id);
        Company? GetProducts(int id);
        Company? GetFull(int id);
    }
}
