using AutoMapper;
using Eleganza.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Models;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Eleganza.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IMapper mapper;
        private readonly IConfiguration configuration;
        private readonly ILogger<AccountController> logger;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<IdentityRole> roleManager, IMapper mapper,
            IConfiguration configuration, ILogger<AccountController> logger
            )
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.roleManager = roleManager;
            this.mapper = mapper;
            this.configuration = configuration;
            this.logger = logger;
        }
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromForm] ApplicationUserDTO userDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (!roleManager.Roles.Any())
            {
                await roleManager.CreateAsync(new IdentityRole(SD.UserRole));
                await roleManager.CreateAsync(new IdentityRole(SD.AdminRole));
            }

            var user = mapper.Map<ApplicationUser>(userDto);
            if (userDto.ProfilePicturePath == null)
            {
                user.ProfilePicturePath = "Images/images.jpeg";
            }
            else
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var extension = Path.GetExtension(userDto.ProfilePicturePath.FileName).ToLower();

                if (!allowedExtensions.Contains(extension))
                    return BadRequest("Invalid file type. Only .jpg, .jpeg, .png, and .gif are allowed.");

                if (userDto.ProfilePicturePath.Length > 2 * 1024 * 1024)
                    return BadRequest("File size exceeds 2MB.");

                var fileName = $"{Guid.NewGuid()}{extension}";
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", "Profile");
                Directory.CreateDirectory(folderPath);
                var filePath = Path.Combine(folderPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await userDto.ProfilePicturePath.CopyToAsync(stream);
                }

                user.ProfilePicturePath = $"/Images/Profile/{fileName}";
            }
            var result = await userManager.CreateAsync(user, userDto.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            await userManager.AddToRoleAsync(user, SD.UserRole);

            var token = await GenerateJwtToken(user);
            return Ok(new
            {
                Token = token,
                Message = "The user has been successfully registered!"
            });

        }
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] Login login)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await userManager.FindByEmailAsync(login.EmailAddress);
            if (user == null)
                return NotFound(new { Message = "Invalid email or password." });

            var result = await userManager.CheckPasswordAsync(user, login.Password);
            if (!result)
                return BadRequest(new { Message = "Invalid email or password." });

            var token = await GenerateJwtToken(user);
            return Ok(new
            {
                Token = token,
                Message = "The user has been successfully logged in!"
            });
        }

        [HttpPost("ChangePassword")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePassword changePassword)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var user = await userManager.FindByEmailAsync(changePassword.Email);
            if (user == null)
            {
                return NotFound(new { Message = "User not found." });
            }
            var isOldPasswordValid = await userManager.CheckPasswordAsync(user, changePassword.OldPassword);
            if (!isOldPasswordValid)
            {
                return BadRequest(new { Message = "Old password is incorrect." });
            }
            var result = await userManager.ChangePasswordAsync(user, changePassword.OldPassword, changePassword.NewPassword);
            if (result.Succeeded)
            {
                return Ok(new { Message = "Password changed successfully." });
            }
            return BadRequest(result.Errors);
        }
        [HttpGet("Profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized(new { error = "Authorization error", message = "User ID is missing or invalid." });

            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound(new { error = "Not Found", message = "The requested user does not exist." });

            var userData = new
            {
                user.UserName,
                user.Email,
                user.PhoneNumber,
                user.Adderss,
                ProfilePicturePath = user.ProfilePicturePath,
                user.Gender
            };

            return Ok(userData);
        }
        [HttpPost("Profile/Update")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromForm] ProfileDTO profile)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized(new { error = "Authorization error", message = "User ID is missing or invalid." });

            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound(new { error = "Not Found", message = "The requested user does not exist." });

            if (!string.IsNullOrEmpty(profile.UserName)) user.UserName = profile.UserName;
            if (!string.IsNullOrEmpty(profile.Email)) user.Email = profile.Email;
            if (!string.IsNullOrEmpty(profile.PhoneNumber)) user.PhoneNumber = profile.PhoneNumber;
            if (!string.IsNullOrEmpty(profile.Adderss)) user.Adderss = profile.Adderss;

            if (profile.ProfilePicture != null && profile.ProfilePicture.Length > 0)
            {
                try
                {
                    Console.WriteLine($"Current Profile Picture Path: {user.ProfilePicturePath}");

                    if (!string.IsNullOrEmpty(user.ProfilePicturePath) &&
                        !user.ProfilePicturePath.Trim().Equals("/images/images.jpeg", StringComparison.OrdinalIgnoreCase))
                    {
                        var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.ProfilePicturePath.TrimStart('/'));
                        Console.WriteLine($"Old File Path: {oldFilePath}");

                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                            Console.WriteLine("Old profile picture deleted.");
                        }
                    }

                    user.ProfilePicturePath = await SaveProfilePicture(profile.ProfilePicture);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error while updating profile picture: {ex.Message}");
                    return BadRequest(new { error = "Image Upload Error", message = ex.Message });
                }
            }

            var result = await userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return Ok(new
                {
                    message = "Profile updated successfully",
                    userData = new
                    {
                        user.UserName,
                        user.Email,
                        user.PhoneNumber,
                        user.Adderss,
                        user.ProfilePicturePath
                    }
                });
            }
            else
            {
                var errors = result.Errors.Select(e => e.Description);
                return BadRequest(new { message = "Failed to update profile.", errors });
            }
        }

        private async Task<string> SaveProfilePicture(IFormFile profilePicture)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(profilePicture.FileName).ToLower();

            if (!allowedExtensions.Contains(extension))
                throw new Exception("Invalid file type. Only .jpg, .jpeg, .png, and .gif are allowed.");

            if (profilePicture.Length > 5 * 1024 * 1024)
                throw new Exception("File size exceeds 5MB.");

            var fileName = $"{Guid.NewGuid()}{extension}";
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", "Profile");

            Directory.CreateDirectory(folderPath);

            var filePath = Path.Combine(folderPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await profilePicture.CopyToAsync(stream);
            }

            return $"/images/Profile/{fileName}";
        }
        [HttpPost("Logout")]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return Ok(new { Message = "User logged out successfully" });
        }

        [HttpDelete("DeleteAccount")]
        [Authorize(Roles = $"{SD.AdminRole},{SD.UserRole}")]

        public async Task<IActionResult> DeleteAccount()
        {
            try
            {
                var userIdClaim = User?.FindFirst(ClaimTypes.NameIdentifier) ?? User?.FindFirst("sub");
                var userId = userIdClaim?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new ProblemDetails
                    {
                        Title = "Unauthorized",
                        Status = StatusCodes.Status401Unauthorized,
                        Detail = "User ID not found in the token."
                    });
                }

                var user = await userManager.Users
                    .Where(u => u.Id == userId)
                    .FirstOrDefaultAsync();
                
                if (user == null)
                {
                    return NotFound(new ProblemDetails
                    {
                        Title = "Not Found",
                        Detail = "User not found."
                    });
                }

              

                if (!string.IsNullOrEmpty(user.ProfilePicturePath))
                {
                    var picturePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "Profile", user.ProfilePicturePath.TrimStart('/'));
                    if (System.IO.File.Exists(picturePath))
                    {
                        System.IO.File.Delete(picturePath);
                        logger.LogInformation("Profile picture for user {UserId} was successfully deleted.", userId);
                    }
                    else
                    {
                        logger.LogWarning("Profile picture for user {UserId} not found at path: {PicturePath}", userId, picturePath);
                    }
                }

                var result = await userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    logger.LogInformation("User {UserId} was successfully deleted.", userId);
                    return Ok(new { message = "Account and related data were successfully deleted." });
                }

                logger.LogWarning("Failed to delete user {UserId}.", userId);
                return BadRequest(new ProblemDetails
                {
                    Title = "Deletion Failed",
                    Detail = "Failed to delete the account."
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while deleting the account for user {UserId}.", User.FindFirst("sub")?.Value);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Server Error",
                    Detail = "An error occurred while processing the request."
                });
            }

        }


        private async Task<string> GenerateJwtToken(ApplicationUser user)
        {
            var userRoles = await userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            };

            foreach (var role in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSettings:SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: configuration["JwtSettings:Issuer"],
                audience: configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(configuration["JwtSettings:ExpiryInMinutes"])),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }


}
