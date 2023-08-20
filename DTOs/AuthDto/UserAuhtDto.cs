using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace BlogAPI.DTOs
{
    public class UserAuthDto
    {
        [Required(ErrorMessage = "Name is required!")]
        public string Name { get; set; }

        [EmailAddress]
        [Required(ErrorMessage = "Email is required!")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required!")]
        [MinLength(6, ErrorMessage = "The password must have at least 6 characters")]
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