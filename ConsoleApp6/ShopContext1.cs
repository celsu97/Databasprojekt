namespace ConsoleApp6;
using Microsoft.EntityFrameworkCore;
using System;
using ConsoleApp6.Models;

public class ShopContext1 : DbContext
{
    //Db<Category> mappar till tabellen category i databasen
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderRow> OrderRows => Set<OrderRow>();
    public DbSet<Product> Products => Set<Product>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // säger vart databasen finns
        var dbPath = Path.Combine(AppContext.BaseDirectory, "shop.db");
        optionsBuilder.UseSqlite ($"Filename={dbPath}");
    }

    //OnModelCreating används för att finjustera modellen
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(e => 
       {
        // Sätter PK
        e.HasKey(c => c.CustomerId);
        
        // Säkerställer samma regler som data annotations (required, MaxLength)
        e.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        e.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(250);
        
        e.HasIndex(x => x.Email)
            .IsUnique();
        
        e.Property(x => x.City)
            .HasMaxLength(250);

        // Kommer tillbaka till detta
        e.HasMany(c => c.Orders);

    });
        modelBuilder.Entity<Order>(e => 
        {
            //PK
            e.HasKey(o => o.OrderId);

            e.Property(x => x.OrderDate)
                .IsRequired();

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

        modelBuilder.Entity<OrderRow>(e => 
        {
            //PK
            e.HasKey(or => or.OrderRowId);

            e.Property(x => x.Quantity)
                .IsRequired();
            e.Property(x => x.UnitPrice)
                .IsRequired();
            
            // Relations: Order 1 - N OrderRows
            e.HasOne(or => or.Order)
                .WithMany(or => or.OrderRows)
                .HasForeignKey(or => or.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Product 1 - M OrderRows, 
            e.HasOne(p => p.Product)
                .WithMany() // Ingen navigation property i Product-klassen
                .HasForeignKey(or => or.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

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

    }

    

}