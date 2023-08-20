using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BlogAPI.DTOs
{
    public class AuthorizationCodeDto
    {
        [Required(ErrorMessage = "Code is required!")]
        public long Code { get; set; }

        [EmailAddress]
        [Required(ErrorMessage = "Email is required!")]
        public string Email { get; set; }
    }
}