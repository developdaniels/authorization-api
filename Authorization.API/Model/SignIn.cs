using System;
using System.ComponentModel.DataAnnotations;

namespace Authorization.API.Model
{
    public class SignIn
    {
        [Required]
        [EmailAddress]
        public String Email { get; set; }

        [Required]
        public String Password { get; set; }
    }
}
