using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Shoping_webapplication_project_.ViewModels
{
    public class AdminEditUserViewModel
    {
        public int UserId { get; set; }

        [Required]
        [StringLength(200)]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(300)]
        public string Email { get; set; }

        [Required]
        [StringLength(30)]
        public string Phone { get; set; }

        [Required]
        [StringLength(40)]
        public string Role { get; set; }
    }
}