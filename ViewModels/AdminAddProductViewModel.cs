using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Shoping_webapplication_project_.ViewModels
{
    public class AdminAddProductViewModel
    {
        [Required]
        [StringLength(200)]
        public string ProductName { get; set; }

        [Required]
        [StringLength(1000)]
        public string Description { get; set; }

        [Required]
        [Range(0.01, 999999999)]
        public decimal Price { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }

        [Required]
        [StringLength(100)]
        public string Category { get; set; }

        public string ImageUrl { get; set; }

        public bool IsActive { get; set; }
    }
}