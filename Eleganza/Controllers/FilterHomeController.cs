using DataAccess.Repository.IRepository;
using Eleganza.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models;

namespace Eleganza.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilterHomeController : ControllerBase
    {
        private readonly IProductRepository productRepository;

        public FilterHomeController(IProductRepository productRepository)
        {
            this.productRepository = productRepository;
        }
        [HttpPost("filter")]
        public IActionResult FilterProducts([FromBody] ProductFilterDTO filter)
        {
            var products = productRepository.Get().AsQueryable();

            if (filter.MinPrice.HasValue)
                products = products.Where(p => p.Price >= filter.MinPrice.Value);

            if (filter.MaxPrice.HasValue)
                products = products.Where(p => p.Price <= filter.MaxPrice.Value);

            if (!string.IsNullOrEmpty(filter.Color))
                products = products.Where(p => p.Color != null && p.Color.Contains(filter.Color));

            if (!string.IsNullOrEmpty(filter.Tag))
                products = products.Where(p => p.Tag != null && p.Tag.Contains(filter.Tag));

            products = filter.SortBy switch
            {
                "PriceAsc" => products.OrderBy(p => p.Price),
                "PriceDesc" => products.OrderByDescending(p => p.Price),
                "Newest" => products.OrderByDescending(p => p.ProductId), 
                _ => products
            };

            var result = products.Select(p => new Product
            {
                ProductId = p.ProductId,
                ProductName = p.ProductName,
                Price = p.Price,
                Color = p.Color,
                Tag = p.Tag,
                ImageUrl = p.ImageUrl
            }).ToList();

            return Ok(result);
        }

    }
}
