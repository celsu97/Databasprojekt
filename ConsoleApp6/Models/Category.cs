using System.ComponentModel.DataAnnotations;

namespace ConsoleApp6.Models;

public class Category
{
    // PK 
    public int CategoryId { get;set;}
    
    [Required, MaxLength(100)]
    public string CategoryName { get;set;}
    
    [Required, MaxLength(250)]
    public string? CategoryDescription { get;set;}
    
    public List<Product> Products { get; set; } = new();
}