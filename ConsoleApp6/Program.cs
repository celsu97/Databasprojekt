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
    Console.WriteLine("4. Orders");
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
        case "4":
            await OrderMenuAsync();
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
                Console.Write("Filter by name (press Enter to show all): ");
                var filter = Console.ReadLine()?.Trim();
                var customers = await db.Customers
                    .Where(c => string.IsNullOrEmpty(filter) || c.Name.Contains(filter))
                    .ToListAsync();
                foreach (var c in customers)
                    Console.WriteLine($"ID: {c.CustomerId} | Name: {c.Name} | Email: {c.Email}");
                break;

            case "2":
                Console.Write("Name: "); var name = Console.ReadLine() ?? "";
                Console.Write("Email: "); var email = Console.ReadLine() ?? "";
                Console.Write("Password: "); var pwd = Console.ReadLine() ?? "";

                if (string.IsNullOrWhiteSpace(name)) { Console.WriteLine("Error: Name is required!"); break; }
                if (string.IsNullOrWhiteSpace(email)) { Console.WriteLine("Error: Email is required!"); break; }
                
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
                    else Console.WriteLine("Error: Customer not found.");
                }
                else Console.WriteLine("Error: Invalid ID.");
                break;

            case "4":
                Console.Write("Enter ID to delete: ");
                if (int.TryParse(Console.ReadLine(), out int id))
                {
                    var cust = await db.Customers.FindAsync(id);
                    if (cust != null) { db.Customers.Remove(cust); await db.SaveChangesAsync(); Console.WriteLine("Customer deleted."); }
                    else Console.WriteLine("Error: Customer not found.");
                }
                else Console.WriteLine("Error: Invalid ID.");
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
                Console.Write("Filter by name (press Enter to show all): ");
                var filter = Console.ReadLine()?.Trim();
                var products = await db.Products
                    .Include(p => p.Category)
                    .Where(p => string.IsNullOrEmpty(filter) || p.ProductName!.Contains(filter))
                    .ToListAsync();
                foreach (var product in products)
                    Console.WriteLine($"ID: {product.ProductId} | {product.ProductName} | {product.Price} SEK | Category: {product.Category?.CategoryName ?? "None"}");
                break;

            case "2":
                var categories = await db.Categories.ToListAsync();
                if (!categories.Any()) { Console.WriteLine("No categories exist. Add a category first."); break; }

                Console.WriteLine("Available categories:");

                foreach (var cat in categories)
                    Console.WriteLine($"  {cat.CategoryId}. {cat.CategoryName}");

                Console.Write("Select Category ID: ");

                if (!int.TryParse(Console.ReadLine(), out int selectedCatId)) { Console.WriteLine("Invalid category ID."); break; }
                
                Console.Write("Name: "); var name = Console.ReadLine();
                Console.Write("Price: ");
                
                if (decimal.TryParse(Console.ReadLine(), out decimal price))
                {
                    if (string.IsNullOrEmpty(name)) { Console.WriteLine("Error: Name is required!"); break; }
                    db.Products.Add(new Product { ProductName = name, Price = price, CategoryId = selectedCatId });
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
                    else Console.WriteLine("Error: Product not found.");
                }
                else Console.WriteLine("Error: Invalid ID.");
                break;

            case "4":
                Console.Write("ID to delete: ");
                if (int.TryParse(Console.ReadLine(), out int id))
                {
                    var prod = await db.Products.FindAsync(id);
                    if (prod != null) { db.Products.Remove(prod); await db.SaveChangesAsync(); Console.WriteLine("Product deleted."); }
                    else Console.WriteLine("Error: Product not found.");
                }
                else Console.WriteLine("Error: Invalid ID.");
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
                    else Console.WriteLine("Error: Category not found.");
                }
                else Console.WriteLine("Error: Invalid ID.");
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
                    else Console.WriteLine("Error: Category not found.");
                }
                else Console.WriteLine("Error: Invalid ID.");
                break;
        }
    }
}

// Menu for Orders
async Task OrderMenuAsync()
{
    while (true)
    {
        Console.WriteLine("\n-- ORDER MANAGEMENT --");
        Console.WriteLine("1. List Orders");
        Console.WriteLine("2. Place New Order");
        Console.WriteLine("B. Back");
        Console.Write("> ");

        var choice = Console.ReadLine()?.Trim().ToLower();
        if (choice == "b") break;

        switch (choice)
        {
            case "1":
                var orders = await db.Orders
                    .Include(o => o.Customer)
                    .Include(o => o.OrderRows)
                    .ThenInclude(or => or.Product)
                    .ToListAsync();
                foreach (var o in orders)
                {
                    Console.WriteLine($"Order ID: {o.OrderId} | Customer: {o.Customer?.Name} | Date: {o.OrderDate:yyyy-MM-dd} | Status: {o.Status}");
                    foreach (var row in o.OrderRows)
                        Console.WriteLine($"  - {row.Product?.ProductName} x{row.Quantity} @ {row.UnitPrice} SEK");
                }
                break;

            case "2":{
                // Show customers
                var customers = await db.Customers.ToListAsync();
                if (!customers.Any()) { Console.WriteLine("No customers found."); break; }
                Console.WriteLine("Select customer:");

                foreach (var c in customers)
                    Console.WriteLine($"  {c.CustomerId}. {c.Name}");
                Console.Write("Customer ID: ");

                if (!int.TryParse(Console.ReadLine(), out int custId)) { Console.WriteLine("Invalid ID."); break; }

                // Show products
                var products = await db.Products.ToListAsync();
                if (!products.Any()) { Console.WriteLine("No products found."); break; }
                Console.WriteLine("Available products:");

                foreach (var p in products)
                    Console.WriteLine($"  {p.ProductId}. {p.ProductName} | {p.Price} SEK");

                // Build order rows
                var orderRows = new List<OrderRow>();
                while (true)
                {
                    Console.Write("Add product ID (or 'done'): ");
                    var input = Console.ReadLine()?.Trim();
                    if (input?.ToLower() == "done") break;
                    if (!int.TryParse(input, out int prodId)) { Console.WriteLine("Invalid ID."); continue; }
                    var product = products.FirstOrDefault(p => p.ProductId == prodId);
                    if (product == null) { Console.WriteLine("Product not found."); continue; }
                    Console.Write("Quantity: ");
                    if (!int.TryParse(Console.ReadLine(), out int qty)) { Console.WriteLine("Invalid quantity."); continue; }
                    orderRows.Add(new OrderRow { ProductId = prodId, Quantity = qty, UnitPrice = product.Price });
                    Console.WriteLine($"Added {product.ProductName} x{qty}");
                }

                if (!orderRows.Any()) { Console.WriteLine("No products added, order cancelled."); break; }

                // TRANSACTION: wrap order placement so it's all-or-nothing
                await using var transaction = await db.Database.BeginTransactionAsync();
                try
                {
                    var order = new Order
                    {
                        CustomerId = custId,
                        OrderDate = DateTime.Now,
                        Status = "New",
                        TotalAmount = orderRows.Sum(r => r.Quantity * r.UnitPrice),
                        OrderRows = orderRows
                    };
                    db.Orders.Add(order);
                    await db.SaveChangesAsync();
                    await transaction.CommitAsync();
                    Console.WriteLine($"Order placed! Order ID: {order.OrderId}, Total: {order.TotalAmount} SEK");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine($"Order failed and was rolled back: {ex.Message}");
                }
                break;
            }
        }
    }
}