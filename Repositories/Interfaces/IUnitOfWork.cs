namespace API.Repositories.Interfaces
{
    public interface IUnitOfWork
    {
        public ICompanyRepository CompanyRepository { get; }
        public IEmployeeRepository EmployeeRepository { get; }
        public IProductRepository ProductRepository { get; }
        Task CommitAsync();
        void Dispose();
    }
}
