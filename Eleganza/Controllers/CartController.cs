using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models;

namespace Eleganza.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly ICartRepository cartRepository;
        private readonly UserManager<ApplicationUser> userManager;

        public CartController(ICartRepository cartRepository, UserManager<ApplicationUser> userManager)
        {
            this.cartRepository = cartRepository;
            this.userManager = userManager;
        }

        [HttpPost]
        public IActionResult AddToCart(int productId, int count)
        {
            var userId = userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { Message = "User not authenticated" });
            }

            if (productId <= 0 || count <= 0)
            {
                return BadRequest(new { Message = "Invalid product ID or count" });
            }

            try
            {
                var existingCart = cartRepository.GetOne(expression: c => c.ProductId == productId && c.ApplicationUserId == userId);

                if (existingCart != null)
                {
                    existingCart.Count += count;
                    cartRepository.Edit(existingCart);
                }
                else
                {
                    var newCart = new Cart
                    {
                        ProductId = productId,
                        Count = count,
                        ApplicationUserId = userId
                    };
                    cartRepository.Create(newCart);
                }

                cartRepository.commit();

                return Ok(new { Message = existingCart != null ? "Product quantity updated in cart" : "Product added to cart successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while adding to cart", Error = ex.Message });
            }
        }
        [HttpGet("All")]
        public IActionResult GetCartItems()
        {
            var userId = userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { Message = "User not authenticated" });
            }
            try
            {
                var cartItems = cartRepository.Get([p=>p.Product, a=>a.ApplicationUser], e=>e.ApplicationUserId == userId );
                if (cartItems == null || !cartItems.Any())
                {
                    return NotFound(new { Message = "No items found in cart" });
                }
                return Ok(cartItems);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while retrieving cart items", Error = ex.Message });
            }
        }

        [HttpPut("Increment")]
        public IActionResult Increment(int productId)
        {
            var userId = userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { Message = "User not authenticated" });
            }
            if (productId <= 0)
            {
                return BadRequest(new { Message = "Invalid product ID" });
            }
            try
            {
                var cartItem = cartRepository.GetOne(expression: c => c.ProductId == productId && c.ApplicationUserId == userId);
                if (cartItem == null)
                {
                    return NotFound(new { Message = "Cart item not found" });
                }
                cartItem.Count++;
                cartRepository.Edit(cartItem);
                cartRepository.commit();
                return Ok(new { Message = "Product quantity incremented successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while incrementing product quantity", Error = ex.Message });
            }
        }
        [HttpPut("Decrement")]
        public IActionResult Decrement(int productId)
        {
            var userId = userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { Message = "User not authenticated" });
            }
            if (productId <= 0)
            {
                return BadRequest(new { Message = "Invalid product ID" });
            }
            try
            {
                var cartItem = cartRepository.GetOne(expression: c => c.ProductId == productId && c.ApplicationUserId == userId);
                if (cartItem == null)
                {
                    return NotFound(new { Message = "Cart item not found" });
                }
                if (cartItem.Count > 1)
                {
                    cartItem.Count--;
                    cartRepository.Edit(cartItem);
                    cartRepository.commit();
                    return Ok(new { Message = "Product quantity decremented successfully" });
                }
                else
                {
                    cartRepository.Delete(cartItem);
                    cartRepository.commit();
                    return Ok(new { Message = "Product removed from cart" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while decrementing product quantity", Error = ex.Message });
            }
        }
        [HttpDelete("{productId}")]
        public IActionResult Delete(int productId)
        {
            var userId = userManager.GetUserId(User);
            var cartItem = cartRepository.GetOne(expression: c => c.ProductId == productId && c.ApplicationUserId == userId);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { Message = "User not authenticated" });
            }
            if (cartItem == null)
            {
                return NotFound(new { Message = "Cart item not found" });
            }
            try
            {
                cartRepository.Delete(cartItem);
                cartRepository.commit();
                return Ok(new { Message = "Product removed from cart successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while deleting the product from cart", Error = ex.Message });
            }
        }





    }
}
