using Microsoft.EntityFrameworkCore;
using DemoWebAPIWithSqlServerInDocker.Models;

public class ProductDbContext : DbContext
    {
        public ProductDbContext (DbContextOptions<ProductDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Product { get; set; } = default!;
    }
