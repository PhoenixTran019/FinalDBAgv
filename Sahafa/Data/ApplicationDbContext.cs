using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Sahafa.Models;

namespace Sahafa.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    public DbSet<Customers> Customers { get; set; }
    public DbSet<AccountCustomers> AccountCustomer { get; set; }
    public DbSet<Book> Book { get; set; }
    public DbSet<Authors> Authors { get; set; }
    public DbSet<Supplier> Supplier { get; set; }
    public DbSet<Stationery> Stationery  { get; set; }
    public DbSet<Employees> Employees { get; set; }
    public DbSet<AccountEmployee> AccountEmployee { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Cấu hình kiểu dữ liệu cho thuộc tính Price của Book
        modelBuilder.Entity<Book>()
            .Property(b => b.Price)
            .HasColumnType("decimal(18,2)");

        // Cấu hình kiểu dữ liệu cho thuộc tính Price của Book
        modelBuilder.Entity<Stationery>()
        .Property(s => s.Price)
        .HasPrecision(18, 2);

        //Config Relation AccountCus and Cus
        modelBuilder.Entity<AccountCustomers>()
            .HasOne(a => a.Customer)
            .WithOne(c => c.AccountCustomer)
            .HasForeignKey<AccountCustomers>(a => a.CustomerID);

        //Config Relation Book and Author
        modelBuilder.Entity<Book>()
            .HasOne(b => b.Authors)
            .WithMany(a => a.Book)
            .HasForeignKey(b => b.AuthorID)
            .OnDelete(DeleteBehavior.Restrict);

        //Config Relation Book and Supplier
        modelBuilder.Entity<Book>()
            .HasOne(b => b.Supplier)
            .WithMany(s => s.Book)
            .HasForeignKey(b => b.SupplierID)
            .OnDelete(DeleteBehavior.Restrict);

        //Config Relation Stationery and Supplier
        modelBuilder.Entity<Stationery>()
            .HasOne(st => st.Supplier)
            .WithMany(s => s.Stationery)
            .HasForeignKey(st => st.SupplierID)
            .OnDelete(DeleteBehavior.Restrict);

        //Congfig Relation Emp and AccEmp
        modelBuilder.Entity<AccountEmployee>()
            .HasOne(a => a.Employee)
            .WithOne()
            .HasForeignKey<AccountEmployee>(a => a.EmployeeID);
    }
}
