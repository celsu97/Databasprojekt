using System.ComponentModel.DataAnnotations;

namespace ConsoleApp6.Models
{
    /// <summary>
    /// Represents a single row/item in an order.
    /// </summary>
    public class OrderRow
    {
        //PK
        public int OrderRowId { get; set; }
        
        //FK
        public int OrderId { get; set; }
        public int ProductId { get; set; }

        // Properties
        [Required]
        public int Quantity { get; set; }

        [Required]
        public decimal UnitPrice { get; set; }

        // Navigation
        public Order? Order { get; set; }
        public Product? Product { get; set; }
    }
}