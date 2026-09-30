using System;
using System.Collections.Generic;

namespace Shoping_webapplication_project_.Models
{
    public class UserOrderItemViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ImageUrl { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }

    public class UserOrderDetailsViewModel
    {
        public Order Order { get; set; }
        public List<UserOrderItemViewModel> Items { get; set; }
    }
}