using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BlogAPI.DTOs
{
    public class ChangeForgottenPasswordDto : AuthorizationCodeDto
    {
        [Required(ErrorMessage = "New password is required!")]
        [MinLength(6, ErrorMessage = "The password must have at least 6 characters")]
        public string NewPassword { get; set; }
    }
}