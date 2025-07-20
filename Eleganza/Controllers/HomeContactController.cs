using DataAccess.Repository;
using DataAccess.Repository.IRepository;
using Eleganza.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models;

namespace Eleganza.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeContactController : ControllerBase
    {
        private readonly IContactUsRepository contactUsRepository;
        private readonly UserManager<ApplicationUser> userManager;

        public HomeContactController(IContactUsRepository contactUsRepository, UserManager<ApplicationUser> userManager)
        {
            this.contactUsRepository = contactUsRepository;
            this.userManager = userManager;
        }
        [Authorize]
        [HttpPost("Create")]
        public async Task<IActionResult> CreateContact([FromBody] ContactUsDTO contact)
        {
            if (contact == null)
            {
                return BadRequest("Contact information is null.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized("User is not logged in.");
            }

            var contactEntity = new ContactUs
            {
                ContactUsId = contact.Id,
                Email = contact.Email,
                Message = contact.Message,
                UserId = user.Id 
            };

            contactUsRepository.Create(contactEntity);
            contactUsRepository.commit();

            return CreatedAtAction(nameof(CreateContact), new { id = contactEntity.ContactUsId }, contactEntity);
        }

    }
}   

