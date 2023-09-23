using System.ComponentModel.DataAnnotations;

namespace BlogAPI.DTOs
{
    public class ChangeForgottenPasswordDto : AuthorizationCodeDto
    {
        [Required(ErrorMessage = "New password is required.")]
        [MinLength(6, ErrorMessage = "The password must be at least 6 characters long.")]
        public string NewPassword { get; set; }
    }
}