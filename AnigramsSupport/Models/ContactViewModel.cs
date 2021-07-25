using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TST.UI.MVC.Models
{
    public class ContactViewModel
    {
        [Required(ErrorMessage = "*Name Required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "*Email Required")]
        [RegularExpression(@"^[A-Za-z0-9](([_\.\-]?[a-zA-Z0-9]+)*)@([A-Za-z0-9]+)(([\.\-]?[a-zA-Z0-9]+)*)\.([A-Za-z]{2,})$", ErrorMessage = "*Invalid email address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "*Subject Required")]
        public string Subject { get; set; }

        [DataType(DataType.MultilineText)]
        public string Message { get; set; }

        [DisplayFormat(DataFormatString = "{0:D} at {0:t}")]
        public System.DateTime DateSent { get; set; }
    }
}