using System.ComponentModel.DataAnnotations;

namespace ConsoleApp6.Models;

public class Product
{
    //PK
    public int ProductId {get; set;}

    // Properties
    [Required]
    public decimal Price {get; set;}

    [MaxLength(250)]
    public string? Description {get; set;}

    [Required, MaxLength(100)]
    public string? ProductName {get; set;}

    // Navigation ?
    public List<OrderRow> OrderRows {get; set;} = new();
    
}

