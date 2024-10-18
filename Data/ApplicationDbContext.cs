using CanamDistributors.Entity;
using Microsoft.EntityFrameworkCore;

namespace CanamDistributors.Data
{
    public class ApplicationDbContext : DbContext
    {
        // Define DbSets for your entities
        public DbSet<AdminEntity> Admin { get; set; }
        public DbSet<CategoryEntity> Category { get; set; }
        public DbSet<Products> Products { get; set; }
        public DbSet<Customer> Customer { get; set; }
        public DbSet<CategoryProducts> CategoryProducts { get; set; }
        public DbSet<ProductImages> ProductImages { get; set; }
        public DbSet<Suppliers> Suppliers { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options): base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

       
    }
}
