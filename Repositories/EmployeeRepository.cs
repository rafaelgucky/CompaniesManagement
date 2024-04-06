using API.Data;
using API.Models;
using API.Pagination;
using API.Pagination.Shared;
using API.Repositories.Interfaces;
using X.PagedList;

namespace API.Repositories
{
    public class EmployeeRepository : Repository<Employee>, IEmployeeRepository
    {
        public EmployeeRepository(AplicationContext context) : base(context) { }

        public IEnumerable<Employee> GetByName(string name)
        {
            return _context.Employees.Where(e => e.Name == name);
        }

        public async Task<IPagedList<Employee>> PaginationGetAsync(EmployeeParameters parameters)
        {
            var allEmployees = await GetAsync();
            var employees = allEmployees.OrderBy(employee => employee.Id).AsQueryable();
            //return PagedList<Employee>.ToPagedList(employees, parameters.PageNumber, parameters.PageSize);
            return await employees.ToPagedListAsync(parameters.PageNumber, parameters.PageSize);
        }
    }
}
