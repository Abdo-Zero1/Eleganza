using DataAccess.Repository.IRepository;
using Eleganza.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;
using Utility;

namespace Eleganza.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles =$"{SD.AdminRole}")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryRepository categoryRepository;

        public CategoryController(ICategoryRepository categoryRepository)
        {
            this.categoryRepository = categoryRepository;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var categories = categoryRepository.Get();
            return Ok(categories);

        }
        [HttpPost]
        public IActionResult CreateCategory([FromBody] CCategory dto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var category = new Category
            {
                CategoryName = dto.Name!,
                Description = dto.Description
            };

            categoryRepository.Create(category);
            categoryRepository.commit();

            return Ok(new
            {
                success = true,
                message = "✔️ Category created successfully.",
                data = new
                {
                    category.CategoryID,
                    category.CategoryName,
                    category.Description
                }
            });
        }
        [HttpGet("{id}")]
        public IActionResult GetCategory(int id)
        {
            var category = categoryRepository.GetOne(expression: c => c.CategoryID == id);
            if (category == null)
            {
                return NotFound(new { success = false, message = "❌ Category not found." });
            }
            return Ok(new
            {
                success = true,
                data = new
                {
                    category.CategoryID,
                    category.CategoryName,
                    category.Description
                }
            });
        }
        [HttpPut("{id}")]
        public IActionResult Edit(int id, [FromBody] ECategory eCategory)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "❌ Invalid data.", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
            }

            var category = categoryRepository.GetOne(expression: c => c.CategoryID == id);
            if (category == null)
            {
                return NotFound(new { success = false, message = "❌ Category not found." });
            }

            category.CategoryName = eCategory.CategoryName;
            category.Description = eCategory.Description;

            categoryRepository.Edit(category);
            categoryRepository.commit();

            return Ok(new { success = true, message = "✅ Category updated successfully." });
        }
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var category = categoryRepository.GetOne(expression: c => c.CategoryID == id);
            if (category == null)
            {
                return NotFound(new { success = false, message = "❌ Category not found." });
            }
            categoryRepository.Delete(category);
            categoryRepository.commit();
            return Ok(new { success = true, message = "✅ Category deleted successfully." });

        }



    }

}

