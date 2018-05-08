using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Authorization.API.Model
{
    public class SignUp
    {
        [Required]
        public String Name { get; set; }

        [Required]
        [EmailAddress]
        public String Email { get; set; }
        
        [Required]
        public String Password { get; set; }

        public List<Telephone> Telephones { get; set; }
    }
}
