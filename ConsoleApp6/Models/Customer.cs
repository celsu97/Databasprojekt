using System.ComponentModel.DataAnnotations;

namespace ConsoleApp6.Models
{
    /// <summary>
    /// Represents a customer.
    /// </summary>
    public class Customer
    {
        //PK
        public int CustomerId { get; set; }

        // Properties
        [Required, MaxLength(100)] public string Name { get; set; } = null!;

        [Required, MaxLength(250)]
        public string Email { get; set; } = null!;
        
        [MaxLength(250)]
        public string? City { get; set; }

        // Navigation
        public List<Order> Orders { get; set; } = new();
        
        // Hash & Salt
        public string? PersonNummerHash { get; set; }
        public string? PersonnummerSalt { get; set; }

        
    }
}
