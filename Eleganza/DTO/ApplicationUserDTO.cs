using System.ComponentModel.DataAnnotations;

namespace Eleganza.DTO
{
    public class ApplicationUserDTO
    {
       public int Id { get; set; }

        [Required(ErrorMessage = "First Name is Required")]
        [MinLength(3, ErrorMessage = "First Name must be at least 3 characters long")]
        [MaxLength(50, ErrorMessage = "First Name must not exceed 50 characters")]
        public string FristName { get; set; }

        [Required(ErrorMessage = "Last Name Is Required")]
        [MinLength(3, ErrorMessage = "Last Name must be at least 3 characters long")]
        [MaxLength(50, ErrorMessage = "Last Name Must Not Exceed 50 Characters")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email Is Required")]
        [DataType(DataType.EmailAddress, ErrorMessage = "Invalid Email Address Format")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Address Is Required")]
        [StringLength(100, ErrorMessage = "Address Must Not Exceed 100 Characters")]
        public string Address { get; set; }
        [Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression(@"^(\+20\d{10}|01\d{9}|\+?[1-9]\d{1,14})$", ErrorMessage = "Invalid phone number format. Must be an Egyptian or international number.")]

        public string PhoneNumber { get; set; }
        [Required(ErrorMessage ="Gender Is Required")]
        public string Gender { get; set; }

        [Required(ErrorMessage = "New password is required.")]
        [MinLength(8, ErrorMessage = "New password must be at least 8 characters long.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
        ErrorMessage = "New password must have at least one uppercase letter, one lowercase letter, one digit, and one special character.")]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(Password))]
        public string ConfirmPassword { get; set; }

        public IFormFile? ProfilePicturePath { get; set; }

    }
}
