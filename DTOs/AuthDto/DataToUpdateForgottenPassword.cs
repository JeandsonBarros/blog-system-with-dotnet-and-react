using System.ComponentModel.DataAnnotations;

namespace BlogAPI.DTOs
{
    public class DataToUpdateForgottenPassword
    {
        [Required(ErrorMessage = "Code is required.")]
        public long Code { get; set; }

        [EmailAddress]
        [Required(ErrorMessage = "Email is required.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "New password is required.")]
        [MinLength(6, ErrorMessage = "The password must be at least 6 characters long.")]
        public string NewPassword { get; set; }
    }
}