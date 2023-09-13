using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PhoenixConquer.Models
{
    public class ContactForm
    {
        [Required(ErrorMessage = "Username is required")]
        [MaxLength(16, ErrorMessage = "Maximum length is 16")]
        [MinLength(4, ErrorMessage = "Minimum length is 4")]
        [RegularExpression(@"^\S*$", ErrorMessage = "No white space allowed")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [MaxLength(30, ErrorMessage = "Maximum length is 30")]
        [MinLength(4, ErrorMessage = "Minimum length is 4")]
        [RegularExpression(@"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" +
                            @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
                            @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$",
                            ErrorMessage = "Email is not valid")]
        public string Email { get; set; }
        [MinLength(4, ErrorMessage = "Minimum length is 4")]
        [Required(ErrorMessage = "Name is required")]
        [MaxLength(16, ErrorMessage = "Maximum length is 16")]
        [RegularExpression(@"^\S*$", ErrorMessage = "No white space allowed")]
        public string Name { get; set; }

        [MinLength(4, ErrorMessage = "Minimum length is 4")]
        [Required(ErrorMessage = "Name is required")]
        [MaxLength(255, ErrorMessage = "Maximum length is 255")]
        public string Message { get; set; }
    }
}
