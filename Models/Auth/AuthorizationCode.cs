using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlogAPI.Models
{
    /// <summary> 
    /// Code for forgotten password reset authorization or email confirmation. 
    /// </summary>
    public class AuthorizationCode
    {
        public int Id { get; set; }

        /// <summary> Code that must be sent to the user's email. </summary>
        public long Code { get; set; }

        /// <summary> Time for code to expire. </summary>
        public DateTime CodeExpires { get; } = DateTime.UtcNow.AddMinutes(15);

        /// <summary> 
        /// Email address to which the code will be sent.
        /// </summary>
        public string Email { get; set; }
    }
}