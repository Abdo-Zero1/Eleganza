using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Models
{
    public class ApplicationUser: IdentityUser
    {
        public string Adderss { get; set; }
        public string ProfilePicturePath { get; set; }
        public string PhoneNumber { get; set; }
        public string Gender { get; set; }

    }
}
