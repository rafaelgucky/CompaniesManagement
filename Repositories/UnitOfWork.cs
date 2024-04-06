using API.Data;
using API.Repositories.Interfaces;

namespace API.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private AplicationContext _context;
        private ICompanyRepository? _companyRepository;
        private IEmployeeRepository? _employeeRepository;
        private IProductRepository? _productRepository;

        public UnitOfWork(AplicationContext context)
        {
            _context = context;
        }

        public ICompanyRepository CompanyRepository
        {
            get
            {
                return _companyRepository = _companyRepository ?? new CompanyRepository(_context);
            }
        }

        public IEmployeeRepository EmployeeRepository
        {
            get
            {
                return _employeeRepository = _employeeRepository ?? new EmployeeRepository(_context);
            }
        }

        public IProductRepository ProductRepository
        {
            get
            {
                return _productRepository = _productRepository ?? new ProductRepository(_context);
            }
        }

        public async Task CommitAsync()
        {
            await _context.SaveChangesAsync();
        }
        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
