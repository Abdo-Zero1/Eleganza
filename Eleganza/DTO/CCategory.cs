using System.ComponentModel.DataAnnotations;

namespace Eleganza.DTO
{
    public class CCategory
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Name is Required")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Description is Required")]
        public string Description { get; set; }
    }
}
