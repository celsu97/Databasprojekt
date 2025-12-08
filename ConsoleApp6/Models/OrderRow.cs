using System.ComponentModel.DataAnnotations;

namespace ConsoleApp6.Models
{
    public class OrderRow
    {
        //PK
        public int OrderRowId { get; set; }
        
        //FK
        public int OrderId { get; set; }
        public string? ProductId { get; set; }

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