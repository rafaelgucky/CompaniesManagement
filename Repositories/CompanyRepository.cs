using API.Data;
using API.Models;
using API.Pagination;
using API.Pagination.Shared;
using API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using X.PagedList;

namespace API.Repositories
{
    public class CompanyRepository : Repository<Company>, ICompanyRepository
    {
        public CompanyRepository(AplicationContext context) : base(context) { }

        public Company? GetEmployees(int id)
        {
            return _context.Companies
                .Where(company => company.Id == id)
                .Include(company => company.Employees)
                .SingleOrDefault();
        }

        public Company? GetFull(int id)
        {
            return _context.Companies
                .Where(company => company.Id == id)
                .Include(company => company.Employees)
                .Include(company => company.Products)
                .SingleOrDefault();
        }

        public Company? GetProducts(int id)
        {
            return _context.Companies
                .Where(company => company.Id == id)
                .Include(company => company.Products)
                .SingleOrDefault();
        }

        public async Task<IPagedList<Company>> PaginationGetAsync(CompanyParameters parameters)
        {
            var allCompanies = await GetAsync();
            var companies = allCompanies.OrderBy(company => company.Id).AsQueryable();
            /*return PagedList<Company>.ToPagedList(companies, parameters.PageNumber, parameters.PageSize);*/
            return await companies.ToPagedListAsync(parameters.PageNumber, parameters.PageSize);
            // PageNumber e PageSize serão utilizados no Take() do método ToPagedList()
        }
    }
}
