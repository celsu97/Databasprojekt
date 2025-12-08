using System.ComponentModel.DataAnnotations;

namespace ConsoleApp6.Models
{
    public class Customer
    {
        //PK
        public int CustomerId { get; set; }

        // Properties
        [Required, MaxLength(100)]
        public string Name { get; set; } 

        [Required, MaxLength(250)]
        public string Email { get; set; } = null!;
        
        [MaxLength(250)]
        public string? City { get; set; }

        // Navigation
        public List<Order> Orders { get; set; } = new();
    }
}