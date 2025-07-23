using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models;
using System.Linq.Expressions;
using Utility;

namespace Eleganza.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactController : ControllerBase
    {
        private readonly IContactUsRepository contactUsRepository;
        private readonly UserManager<ApplicationUser> userManager;

        public ContactController(IContactUsRepository contactUsRepository, UserManager<ApplicationUser> userManager)
        {
            this.contactUsRepository = contactUsRepository;
            this.userManager = userManager;
        }

        [HttpGet("GetAdmin")]
        [Authorize(Roles = $"{SD.AdminRole},{SD.UserRole}")]
        public IActionResult GetAllContacts()
        {
            var contacts = contactUsRepository.Get(
                new Expression<Func<ContactUs, object>>[] { c => c.User },
                null,
                true
            ).Select(c => new
            {
                id = c.ContactUsId,
                UserName = c.User?.UserName,
                Email = c.User?.Email,
                phone = c.User?.PhoneNumber,
                Address = c.User?.Adderss,
                Message = c.Message,

            }).ToList();
            return Ok(contacts);

        }
    }
}
