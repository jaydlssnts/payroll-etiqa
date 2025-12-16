using Microsoft.EntityFrameworkCore;
using payroll.model;

namespace payroll;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Employee> Employee { get; set; } = null!;
    
}
