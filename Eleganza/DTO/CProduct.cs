using Eleganza.DTO;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Models;


namespace Eleganza.DTO 
{
    public class CProduct
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Product name is required.")]
        [StringLength(100, ErrorMessage = "Product name cannot exceed 100 characters.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Product description is required.")]
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Price is required.")]
        [Range(0.01, 100000, ErrorMessage = "Price must be between 0.01 and 100,000.")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Quantity is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int quantity { get; set; }

        [Required(ErrorMessage = "Color is required.")]
        public string Color { get; set; }
        [Required(ErrorMessage = "Tag is required.")]
        public string Tag { get; set; }
        [Range(0, 5, ErrorMessage = "Rating must be between 0 and 5.")]
        public int Rating { get; set; } = 0;

        public IFormFile? ImageUrl { get; set; }

        [Required(ErrorMessage = "Category is required.")]
        public int CategoryID { get; set; }
        
    }
}