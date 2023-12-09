using System.ComponentModel.DataAnnotations;

namespace TaskHub_V1.Models
{
    public class ChangeEmailModel
    {
        [Required(ErrorMessage = "New email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string NewEmail { get; set; }
        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; }
    }
}
