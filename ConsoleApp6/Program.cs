
using ConsoleApp6;
using ConsoleApp6.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

// Initialize database and apply migrations to ensure the schema is up to date
Console.WriteLine("DB: " + Path.Combine(AppContext.BaseDirectory, "shop.db"));
await using var db = new ShopContext1();
await db.Database.MigrateAsync();

// SEEDING: Prepare the database with initial data if empty. 
// This ensures the application has content to display on first run.
if (!await db.Categories.AnyAsync())
{
    var defaultCategory = new Category 
    { 
        CategoryName = "Electronics", 
        CategoryDescription = "Devices and gadgets" 
    };
    db.Categories.Add(defaultCategory);
    await db.SaveChangesAsync();

    if (!await db.Products.AnyAsync())
    {
        db.Products.AddRange(
            new Product { ProductName = "Smartphone", Price = 5000, Description = "Modern phone" },
            new Product { ProductName = "Laptop", Price = 12000, Description = "Powerful laptop" }
        );
        await db.SaveChangesAsync();
    }
}

if (!await db.Customers.AnyAsync())
{
    db.Customers.AddRange(
        new Customer { Name = "Kalle Karlsson", Email = "kalle@mail.com", City = "Stockholm" },
        new Customer { Name = "Anna Andersson", Email = "anna@mail.com", City = "Göteborg" }
    );
    await db.SaveChangesAsync();
}

// Main Menu: the primary navigation loop of the CLI
while (true)
{
    Console.WriteLine("\n--- MAIN MENU ---");
    Console.WriteLine("1. Customers");
    Console.WriteLine("2. Products");
    Console.WriteLine("3. Categories");
    Console.WriteLine("Type 'exit' to quit.");
    Console.Write("> ");

    var input = Console.ReadLine()?.Trim().ToLower();
    if (input == "exit") break;

    switch (input)
    {
        case "1":
            await CustomerMenuAsync();
            break;
        case "2":
            await ProductMenuAsync();
            break;
        case "3":
            await CategoryMenuAsync();
            break;
        default:
            Console.WriteLine("Unknown command.");
            break;
    }
}

// --- SUB-Menus  ---

// Menu for Customer
async Task CustomerMenuAsync()
{
    while (true)
    {
        Console.WriteLine("\n-- CUSTOMER MANAGEMENT --");
        Console.WriteLine("1. List Customers");
        Console.WriteLine("2. Add Customer");
        Console.WriteLine("3. Edit Customer");
        Console.WriteLine("4. Delete Customer");
        Console.WriteLine("B. Back");
        Console.Write("> ");

        var choice = Console.ReadLine()?.Trim().ToLower();
        if (choice == "b") break;

        // CRUD
        switch (choice)
        {
            case "1":
                var customers = await db.Customers.ToListAsync();
                foreach (var c in customers) 
                    Console.WriteLine($"ID: {c.CustomerId} | Name: {c.Name} | Email: {c.Email}");
                break;

            case "2":
                Console.Write("Name: "); var name = Console.ReadLine() ?? "";
                Console.Write("Email: "); var email = Console.ReadLine() ?? "";
                Console.Write("Password: "); var pwd = Console.ReadLine() ?? "";

                // SECURITY: Implementing Password Hashing with Salt
                // We use HMACSHA512 to ensure passwords are never stored in plain text.
                using (var hmac = new HMACSHA512())
                {
                    var salt = Convert.ToBase64String(hmac.Key);
                    var hash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(pwd)));
                    
                    // Storing the hash and salt in the designated model fields
                    db.Customers.Add(new Customer 
                    { 
                        Name = name, 
                        Email = email, 
                        PersonnummerSalt = salt, 
                        PersonNummerHash = hash 
                    });
                }
                await db.SaveChangesAsync();
                Console.WriteLine("Customer created securely.");
                break;

            case "3": 
                Console.Write("Enter Customer ID to edit: ");
                if (int.TryParse(Console.ReadLine(), out int custId))
                {
                    var cust = await db.Customers.FindAsync(custId);
                    if (cust != null)
                    {
                        Console.Write($"New Name (current: {cust.Name}): ");
                        var newName = Console.ReadLine();
                        if (!string.IsNullOrEmpty(newName)) cust.Name = newName;

                        Console.Write("New Password (leave empty to keep current): ");
                        var newPwd = Console.ReadLine();
                        if (!string.IsNullOrEmpty(newPwd))
                        {
                            using var hmac = new System.Security.Cryptography.HMACSHA512();
                            cust.PersonnummerSalt = Convert.ToBase64String(hmac.Key);
                            cust.PersonNummerHash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(newPwd)));
                        }
                        await db.SaveChangesAsync();
                        Console.WriteLine("Customer updated.");
                    }
                }
                break;

            case "4":
                Console.Write("Enter ID to delete: ");
                if (int.TryParse(Console.ReadLine(), out int id))
                {
                    var cust = await db.Customers.FindAsync(id);
                    if (cust != null) { db.Customers.Remove(cust); await db.SaveChangesAsync(); Console.WriteLine("Deleted."); }
                    else Console.WriteLine("Customer not found."); // User feedback for failed operations
                }
                break;
        }
    }
}

// Menu for Products
async Task ProductMenuAsync()
{
    while (true)
    {
        Console.WriteLine("\n-- PRODUCT MANAGEMENT --");
        Console.WriteLine("1. List Products");
        Console.WriteLine("2. Add Product");
        Console.WriteLine("3. Edit Product");
        Console.WriteLine("4. Delete Product");
        Console.WriteLine("B. Back");
        Console.Write("> ");

        var choice = Console.ReadLine()?.Trim().ToLower();
        if (choice == "b") break;

        // CRUD
        switch (choice)
        {
            case "1":
                var products = await db.Products.ToListAsync();
                foreach (var product in products) 
                    Console.WriteLine($"ID: {product.ProductId} | {product.ProductName} | {product.Price} SEK");
                break;

            case "2":
                Console.Write("Name: "); var name = Console.ReadLine();
                Console.Write("Price: ");
                if (decimal.TryParse(Console.ReadLine(), out decimal price))
                {
                    // VALIDATION: Ensure mandatory fields are populated before saving to DB
                    if (string.IsNullOrEmpty(name)) { Console.WriteLine("Error: Name is required!"); break; }
                    db.Products.Add(new Product { ProductName = name, Price = price });
                    await db.SaveChangesAsync();
                    Console.WriteLine("Product added.");
                }
                break;
            case "3":
                Console.Write("Enter Product ID to edit: ");
                if (int.TryParse(Console.ReadLine(), out int prodid))
                {
                    var prod = await db.Products.FindAsync(prodid);
                    if (prod != null)
                    {
                        Console.Write($"New Name (current: {prod.ProductName}): ");
                        var n = Console.ReadLine();
                        if (!string.IsNullOrEmpty(n)) prod.ProductName = n;

                        Console.Write($"New Price (current: {prod.Price}): ");
                        if (decimal.TryParse(Console.ReadLine(), out decimal prodPrice)) prod.Price = prodPrice;

                        await db.SaveChangesAsync();
                        Console.WriteLine("Product updated.");
                    }
                }
                break;

            case "4":
                Console.Write("ID to delete: ");
                if (int.TryParse(Console.ReadLine(), out int id))
                {
                    var prod = await db.Products.FindAsync(id);
                    if (prod != null) { db.Products.Remove(prod); await db.SaveChangesAsync(); Console.WriteLine("Deleted."); }
                }
                break;
        }
    }
}

// Menu for Category
static async Task CategoryMenuAsync()
{
    await using var db = new ShopContext1();
    while (true)
    {
        Console.WriteLine("\n-- CATEGORY MANAGEMENT --");
        Console.WriteLine("1. List Categories");
        Console.WriteLine("2. Add Category");
        Console.WriteLine("3. Edit Category");
        Console.WriteLine("4. Delete Category");
        Console.WriteLine("B. Back");
        Console.Write("> ");

        var choice = Console.ReadLine()?.Trim().ToLower();
        if (choice == "b") break;

        // CRUD
        switch (choice)
        {
            case "1":
                var cats = await db.Categories.ToListAsync();
                foreach (var c in cats) 
                    Console.WriteLine($"ID: {c.CategoryId} | Name: {c.CategoryName}");
                break;

            case "2":
                
                Console.Write("Category Name: ");
                var name = Console.ReadLine();
                Console.Write("Category Description: ");
                var desc = Console.ReadLine();
                
                // Validate input to prevent database exceptions
                if (string.IsNullOrWhiteSpace(name))
                {
                    Console.WriteLine("Error: Category Name is required!");
                    break;
                }
                
                db.Categories.Add(new Category 
                { 
                    CategoryName = name, 
                    CategoryDescription = desc ?? "No description" 
                });
                
                await db.SaveChangesAsync();
                Console.WriteLine("Category saved successfully!");
                break;
            
            case "3":
                Console.Write("Enter Category ID to edit: ");
                if (int.TryParse(Console.ReadLine(), out int Catid))
                {
                    var cat = await db.Categories.FindAsync(Catid);
                    if (cat != null)
                    {
                        Console.Write($"New Name (current: {cat.CategoryName}): ");
                        var cn = Console.ReadLine();
                        if (!string.IsNullOrEmpty(cn)) cat.CategoryName = cn;

                        await db.SaveChangesAsync();
                        Console.WriteLine("Category updated.");
                    }
                }
                break;
            
            case "4":
                Console.Write("Enter ID to delete: ");
                if (int.TryParse(Console.ReadLine(), out int id))
                {
                    var cat = await db.Categories.FindAsync(id);
                    if (cat != null) 
                    {
                        db.Categories.Remove(cat);
                        await db.SaveChangesAsync();
                        Console.WriteLine("Category deleted.");
                    }
                    else Console.WriteLine("Category not found.");
                }
                break;
        }
    }
}
