using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;

namespace Swish.Areas.Identity.Data
{
    // Add profile data for application users by adding properties to the SwishUser class
    public class SwishUser : IdentityUser
    {
        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "First name")]
        public string FName { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Last name")]
        public string LName { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Birth Date")]
        public DateTime DOB { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Profile Picture")]
        public string PPicPath { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Gender")]
        public string Gender { get; set; }
        
        [Display(Name = "Admin")]
        public bool IsAdmin { get; set; }
    }
}
