using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PhoenixConquer.Models
{
    public class ChangePassModel
    {

        [Required(ErrorMessage = "Old password is required")]
        [DataType("Password")]
        [MaxLength(16, ErrorMessage = "Maximum length is 16")]
        [MinLength(4, ErrorMessage = "Minimum length is 4")]
        [RegularExpression(@"^\S*$", ErrorMessage = "No white space allowed")]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType("Password")]
        [MaxLength(14, ErrorMessage = "Maximum length is 14")]
        [MinLength(4, ErrorMessage = "Minimum length is 4")]
        [RegularExpression(@"^\S*$", ErrorMessage = "No white space allowed")]
        public string Password { get; set; }

        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}