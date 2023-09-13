using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PhoenixConquer.Models
{
    public class RegisterAccount
    {
        [Required(ErrorMessage = "Username is required")]
        [MaxLength(16, ErrorMessage = "Maximum length is 16")]
        [MinLength(4, ErrorMessage = "Minimum length is 4")]
        [RegularExpression(@"^\S*$", ErrorMessage = "No white space allowed")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType("Password")]
        [MaxLength(14, ErrorMessage = "Maximum length is 14")]
        [MinLength(4, ErrorMessage = "Minimum length is 4")]
        [RegularExpression(@"^\S*$", ErrorMessage = "No white space allowed")]
        public string Password { get; set; }

        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [MaxLength(50, ErrorMessage = "Maximum length is 50")]
        [MinLength(4, ErrorMessage = "Minimum length is 4")]
        [RegularExpression(@"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" +
                            @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
                            @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$",
                            ErrorMessage = "Email is not valid")]
        public string Email { get; set; }
    }
}