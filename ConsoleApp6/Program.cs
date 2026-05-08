using ConsoleApp6;
using ConsoleApp6.Models;
using Microsoft.EntityFrameworkCore;

Console.WriteLine("DB:" + Path.Combine(AppContext.BaseDirectory, "shop.db"));
// säkerställ DB + migrations + seed
await using var db = new ShopContext1();

{
    // Migrate Async: Skapar databasen om den inte finns
    // Kör bara om det inte finns några kategorier sen innan
    // dotnet ef database update to apply the latest migrations
    await db.Database.MigrateAsync();

    // enkel seeding för databasen
    // kör bara om det inte finns några kategorier sen innan
    if (!await db.Products.AnyAsync())
    {
        db.Products.AddRange(
            new Product { ProductName = "Produkt 1", Price = 100, Description = "Den första produkten" },
            new Product { ProductName = "Produkt 2", Price = 200, Description = "Den andra produkten" },
            new Product { ProductName = "Produkt 3", Price = 300, Description = "Den tredje produkten" }
        );
        // Save database updates
        await db.SaveChangesAsync();
        Console.WriteLine("Databasen seedad med produkter");
    }

    if (!await db.Customers.AnyAsync())
    {
        db.Customers.AddRange(
            new Customer { Name = "Kalle Karlsson", Email = "kalle@mail.com", City = "Stockholm" },
            new Customer { Name = "Anna Andersson", Email = "anna@mail.com", City = "Göteborg" }
        );
    }
    
    if (!await db.Orders.AnyAsync())
    {
        var customer = await db.Customers.FirstAsync();
        db.Orders.Add(
            new Order
            {
                CustomerId = customer.CustomerId,
                OrderDate = DateTime.Now,
                Status = "Ny",
                OrderRows = new List<OrderRow>
                {
                    new OrderRow { ProductId = 1, Quantity = 2, UnitPrice = 100 },
                    new OrderRow { ProductId = 2, Quantity = 1, UnitPrice = 200 }
                }
            }
        );
    }
    await db.SaveChangesAsync();

    // CLI för CRUD; CREATE, READ, UPDATE, DELETE
    while (true)
    {
        Console.WriteLine("| list | add | edit | delete | exit |");
        Console.Write("> ");
        var line = Console.ReadLine()?.Trim() ?? string.Empty;

        // hoppa över tomma rader
        if (string.IsNullOrEmpty(line))
        {
            continue;
        }

        //avslutar programmet
        if (line.Equals("exit",StringComparison.OrdinalIgnoreCase))
        {
            break;
        }

        // Delar upp raden på mellanslag: tex "add product 1" -> ["add","product","1"]
        var parts = line.Split(' ',StringSplitOptions.RemoveEmptyEntries);
        var cmd = parts[0].ToLowerInvariant();

        // enkel switch för kommandon
        switch (cmd)
        {
            case "list":
                await ListCustomersAsync();
                break;
            case "add":
                break;
            case "edit":
                break;
            case "delete":
                break;
            default:
                Console.WriteLine("Okänt kommando: " + cmd);
                break;
        }
    }
    static async Task ListCustomersAsync()
    {
        using var db = new ShopContext1();

        var customers = await db.Customers.AsNoTracking().ToListAsync();
        foreach (var customer in customers)
        {
            Console.WriteLine($"| {customer.CustomerId} | {customer.Name} ({customer.Email}) | {customer.City} | ");
        }
    }
}
