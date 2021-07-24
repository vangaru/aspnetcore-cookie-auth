using System.ComponentModel.DataAnnotations;

namespace CookieAuth.Models
{
    public class RegisterModel
    {
        [Required(ErrorMessage = "Email not set")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        
        [Required(ErrorMessage = "Username not set")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Password not set")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string PasswordConfirmation { get; set; }
    }
}