using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace BlogAPI.DTOs
{
    public class UserAuthDto
    {
        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; }

        [EmailAddress]
        [Required(ErrorMessage = "Email is required.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(6, ErrorMessage = "The password must be at least 6 characters long.")]
        public string Password { get; set; }

        public IFormFile? FileProfilePicture { get; set; }
    }

    [ValidateNever]
    public class UserDtoNoValidation
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public IFormFile FileProfilePicture { get; set; }
    }

}