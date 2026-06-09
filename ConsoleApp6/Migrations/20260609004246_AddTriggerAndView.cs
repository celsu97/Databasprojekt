using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConsoleApp6.Migrations
{
    /// <inheritdoc />
    public partial class AddTriggerAndView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // VIEW: Order summary with customer name and total
            migrationBuilder.Sql(@"
                CREATE VIEW IF NOT EXISTS OrderSummary AS
                SELECT 
                    o.OrderId,
                    c.Name AS CustomerName,
                    o.OrderDate,
                    o.Status,
                    o.TotalAmount
                FROM Orders o
                JOIN Customers c ON o.CustomerId = c.CustomerId;
            ");

            // TRIGGER: Update TotalAmount on Order when an OrderRow is inserted
            migrationBuilder.Sql(@"
                CREATE TRIGGER IF NOT EXISTS UpdateOrderTotal
                AFTER INSERT ON OrderRows
                BEGIN
                    UPDATE Orders
                    SET TotalAmount = (
                        SELECT SUM(Quantity * UnitPrice)
                        FROM OrderRows
                        WHERE OrderId = NEW.OrderId
                    )
                    WHERE OrderId = NEW.OrderId;
                END;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW IF EXISTS OrderSummary;");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS UpdateOrderTotal;");
        }
    }
}
