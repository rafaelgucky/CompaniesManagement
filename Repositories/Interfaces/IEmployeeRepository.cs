using API.Models;
using API.Pagination;
using API.Pagination.Shared;
using X.PagedList;

namespace API.Repositories.Interfaces
{
    public interface IEmployeeRepository : IRepository<Employee>
    {
        Task<IPagedList<Employee>> PaginationGetAsync(EmployeeParameters parameters);
        IEnumerable<Employee> GetByName(string name);
    }
}
