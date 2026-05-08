using System.ComponentModel.DataAnnotations;

namespace ConsoleApp6.Models
{
    public class Order
    {
        //PK
        public int OrderId { get; set; }

        //FK
        public int CustomerId { get; set; }

        // Properties
        public DateTime OrderDate { get; set; }

        [Required, StringLength(50)]
        public string? Status { get; set; }
        
        public decimal TotalAmount {get; set;}

        // Navigation
        public Customer? Customer { get; set; }
        public List<OrderRow> OrderRows { get; set; } = new();
    }
}