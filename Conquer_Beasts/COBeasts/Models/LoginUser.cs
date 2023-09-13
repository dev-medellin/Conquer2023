using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PhoenixConquer.Models
{
    public class LoginUser
    {
        [Required(ErrorMessage = "Username is required")]
        [MaxLength(16, ErrorMessage = "Maximum length is 16")]
        [MinLength(4, ErrorMessage = "Minimum length is 4")]
        [RegularExpression(@"^\S*$", ErrorMessage = "No white space allowed")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType("Password")]
        [MaxLength(16, ErrorMessage = "Maximum length is 16")]
        [MinLength(4, ErrorMessage = "Minimum length is 4")]
        [RegularExpression(@"^\S*$", ErrorMessage = "No white space allowed")]
        public string Password { get; set; }
    }
}
