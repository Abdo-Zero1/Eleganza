using DataAccess.Repository;
using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;
using Models;
using System.Linq.Expressions;

namespace Eleganza.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly ICategoryRepository categoryRepository;
        private readonly IProductRepository productRepository;

        public HomeController(ICategoryRepository categoryRepository, IProductRepository productRepository)
        {
            this.categoryRepository = categoryRepository;
            this.productRepository = productRepository;
        }

        [HttpGet("categories-with-products")]
        public IActionResult GetCategoriesWithProducts()
        {
            try
            {
                var categories = categoryRepository.Get(
                    Include: new Expression<Func<Category, object>>[]
                    {
                c => c.Products
                    });

                if (categories == null || !categories.Any())
                {
                    return NotFound(new { Message = "No categories found." });
                }

                var result = categories.Select(c => new
                {
                    CategoryId = c.CategoryID,
                    CategoryName = c.CategoryName,
                    Description = c.Description,
                    Products = (c.Products != null && c.Products.Any())
         ? c.Products.Select(p => new
         {
             ProductId = p.ProductId,
             Name = p.ProductName,
             Description = p.ProductDescription,
             Price = p.Price,
             ImageUrl = p.ImageUrl
         }).ToList<object>() 
         : new List<object>()
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error fetching categories with products", Error = ex.Message });
            }
        }

        [HttpGet("products")]
        public IActionResult GetProducts([FromQuery] string? categoryName = null)
        {
            try
            {
                var products = productRepository.Get(
                    Include: new Expression<Func<Product, object>>[]
                    {
                p => p.Category
                    },
                    expression: !string.IsNullOrEmpty(categoryName)
                        ? p => p.Category.CategoryName.ToLower() == categoryName.ToLower()
                        : null
                );

                var result = products.Select(p => new
                {
                    ProductId = p.ProductId,
                    Name = p.ProductName,
                    Description = p.ProductDescription,
                    Price = p.Price,
                    ImageUrl = p.ImageUrl,
                    CategoryName = p.Category?.CategoryName
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error fetching products", Error = ex.Message });
            }
        }



    }
}
