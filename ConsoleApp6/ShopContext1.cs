namespace ConsoleApp6;
using Microsoft.EntityFrameworkCore;
using System;
using ConsoleApp6.Models;

/// <summary>
/// Represents the Entity Framework Core database context for the shop application.
/// </summary>

public class ShopContext1 : DbContext
{
    //Db<Category> maps to table category in database
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderRow> OrderRows => Set<OrderRow>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Tells where the database is
        var dbPath = Path.Combine(AppContext.BaseDirectory, "shop.db");
        optionsBuilder.UseSqlite ($"Filename={dbPath}");
    }

    /// <summary>
    /// OnModelCreating used to adjust models
    /// </summary>
    /// <param name="modelBuilder"></param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Customer 
        modelBuilder.Entity<Customer>(e => 
       {
        // Puts PK
        e.HasKey(c => c.CustomerId);
        
        // Ensures same rules as data annotations (required, MaxLength)
        e.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        e.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(100);
        
        e.HasIndex(x => x.Email)
            .IsUnique();
        
        e.Property(x => x.City)
            .HasMaxLength(250);
    });
        
        // Order
        modelBuilder.Entity<Order>(e => 
        {
            //PK
            e.HasKey(o => o.OrderId);

            e.Property(x => x.OrderDate);

            e.Property(x => x.Status)
                .IsRequired()
                .HasMaxLength(50);
            
            e.Property(x => x.TotalAmount)
                .IsRequired();


            // Relations 1 - N Orders
            

            e.HasOne(o => o.Customer)
                .WithMany(o => o.Orders)
                .HasForeignKey(or => or.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        // Order Row
        modelBuilder.Entity<OrderRow>(e => 
        {
            //PK
            e.HasKey(or => or.OrderRowId);

            e.Property(x => x.Quantity)
                .IsRequired();
            e.Property(x => x.UnitPrice)
                .IsRequired();
            
            e.HasOne(or => or.Order)
                .WithMany(or => or.OrderRows)
                .HasForeignKey(or => or.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
            
            e.HasOne(p => p.Product)
                .WithMany() 
                .HasForeignKey(or => or.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        // Product
        modelBuilder.Entity<Product>(e => 
        {
            //PK
            e.HasKey(p => p.ProductId);

            // Properties
            e.Property(x => x.ProductName)
                .IsRequired()
                .HasMaxLength(200);
            
            e.Property(x => x.Description)
                .HasMaxLength(250);
            
            e.Property(x => x.Price)
                .IsRequired();
        });
        
        // Category
        modelBuilder.Entity<Category>(e =>
        {
            e.HasKey(c => c.CategoryId);
            
            e.Property(x => x.CategoryName)
                .IsRequired()
                .HasMaxLength(100);

            e.Property(x => x.CategoryDescription)
                .HasMaxLength(250);
        });
    }
}