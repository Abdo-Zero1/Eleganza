using DataAccess.Repository.IRepository;
using Eleganza.DTO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models;
using System.Linq.Expressions;

namespace Eleganza.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductRepository productRepository;
        private readonly ICategoryRepository categoryRepository;

        public ProductController(IProductRepository productRepository, ICategoryRepository categoryRepository)
        {
            this.productRepository = productRepository;
            this.categoryRepository = categoryRepository;
        }
        [HttpGet]
        public IActionResult Index()
        {
            var products = productRepository.Get();
            var categories = categoryRepository.Get().ToDictionary(c => c.CategoryID, c => c.CategoryName);

            var productList = products.Select(product => new
            {
                product.ProductId,
                product.ProductName,
                product.ProductDescription,
                product.Price,
                product.ImageUrl,
                product.CategoryID,
                CategoryName = categories.ContainsKey(product.CategoryID) ? categories[product.CategoryID] : "Unknown"
            });

            return Ok(productList);
        }


        [HttpPost]
        public IActionResult CreateProduct([FromForm] CProduct dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            string? imagePath = null;

            if (dto.ImageUrl != null && dto.ImageUrl.Length > 0)
            {
                var uploadFile = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", "Product");

                if (!Directory.Exists(uploadFile))
                {
                    Directory.CreateDirectory(uploadFile);
                }

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(dto.ImageUrl.FileName);
                var physicalPath = Path.Combine(uploadFile, fileName);

                using (var fileStream = new FileStream(physicalPath, FileMode.Create))
                {
                    dto.ImageUrl.CopyTo(fileStream);
                }

                imagePath = "/Images/Product/" + fileName;
            }

            var product = new Product
            {
                ProductName = dto.Name!,
                ProductDescription = dto.Description,
                Price = dto.Price,
                quantity = dto.quantity,
                CategoryID = dto.CategoryID,
                ImageUrl = imagePath
            };

            productRepository.Create(product);
            productRepository.commit();

            return Ok(new
            {
                success = true,
                message = "✔️ Product created successfully.",
                data = new
                {
                    product.ProductId,
                    product.ProductName,
                    product.ProductDescription,
                    product.Price,
                    product.ImageUrl,
                    product.CategoryID
                }
            });
        }

        [HttpGet("{id}")]
        public IActionResult GetProduct(int id)
        {
            var product = productRepository.GetOne(expression: p => p.ProductId == id);
            if (product == null)
            {
                return NotFound(new { success = false, message = "❌ Product not found." });
            }

            var category = categoryRepository.GetOne(expression: c => c.CategoryID == product.CategoryID);
            string categoryName = category?.CategoryName ?? "Unknown";

            return Ok(new
            {
                success = true,
                data = new
                {
                    product.ProductId,
                    product.ProductName,
                    product.ProductDescription,
                    product.Price,
                    product.ImageUrl,
                    product.CategoryID,
                    CategoryName = categoryName
                }
            });
        }


        [HttpPut("{id}")]
        public IActionResult UpdateProduct(int id, [FromForm] EProduct dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var product = productRepository.GetOne(expression: p => p.ProductId == id);
            if (product == null)
            {
                return NotFound(new { success = false, message = "❌ Product not found." });
            }

            string? imagePath = null;
            if (dto.ImageUrl != null && dto.ImageUrl.Length > 0)
            {
                var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", "Product");
                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(dto.ImageUrl.FileName);
                var physicalPath = Path.Combine(uploadFolder, fileName);

                using (var stream = new FileStream(physicalPath, FileMode.Create))
                {
                    dto.ImageUrl.CopyTo(stream);
                }

                imagePath = "/Images/Product/" + fileName;
            }

            product.ProductName = dto.Name!;
            product.ProductDescription = dto.Description;
            product.Price = dto.Price;
            product.quantity = dto.quantity;
            product.CategoryID = dto.CategoryID;
            product.ImageUrl = imagePath ?? product.ImageUrl;

            productRepository.Edit(product);
            productRepository.commit();

            return Ok(new
            {
                success = true,
                message = "✔️ Product updated successfully.",
                data = new
                {
                    product.ProductId,
                    product.ProductName,
                    product.ProductDescription,
                    product.Price,
                    product.ImageUrl,
                    product.CategoryID
                }
            });
        }

        [HttpGet("Photo/{ProductId}")]
        public IActionResult GetPhotoUrl(int ProductId)
        {
            var product = productRepository.GetOne(expression: p => p.ProductId == ProductId);
            if (product == null)
            {
                return NotFound(new { Message = "Product Not Found" });
            }

            var imageUrl = !string.IsNullOrEmpty(product.ImageUrl)
                ? $"{Request.Scheme}://{Request.Host}{product.ImageUrl}"
                : null;

            return Ok(new { Url = imageUrl });
        }


        [HttpDelete("{id}")]
        public IActionResult DeleteProduct(int id)
        {
            var product = productRepository.GetOne(expression: p => p.ProductId == id);
            if (product == null)
            {
                return NotFound(new { success = false, message = "❌ Product not found." });
            }

            if (!string.IsNullOrEmpty(product.ImageUrl))
            {
                var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", product.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            productRepository.Delete(product);
            productRepository.commit();

            return Ok(new { success = true, message = "✔️ Product deleted successfully." });
        }

    }
}
