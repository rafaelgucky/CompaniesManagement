using Microsoft.EntityFrameworkCore;
using API.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace API.Data;

public class AplicationContext : IdentityDbContext<ApplicationUser>
{
    public AplicationContext(DbContextOptions<AplicationContext> options) : base(options) { }
    public DbSet<Company> Companies { get; set;}
    public DbSet<Employee> Employees { get; set;}
    public DbSet<Product> Products { get; set;}

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
    }
}
