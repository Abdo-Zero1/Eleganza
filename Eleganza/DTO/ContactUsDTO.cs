using System.ComponentModel.DataAnnotations;

namespace Eleganza.DTO
{
    public class ContactUsDTO
    {
        public int Id { get; set; }

        [Required,EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Message { get; set; }
    }
}
