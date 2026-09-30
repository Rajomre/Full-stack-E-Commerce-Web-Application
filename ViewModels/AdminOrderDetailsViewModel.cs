using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Shoping_webapplication_project_.ViewModels
{
    public class AdminOrderDetailsViewModel
    {
        public string OrderNumber { get; set; }

        public string CustomerName { get; set; }

        public string CustomerEmail { get; set; }

        public string Status { get; set; }

        public DateTime OrderDate { get; set; }

        public decimal TotalAmount { get; set; }
        public int OrderId { get; set; }


        public List<AdminOrderItemViewModel> Items { get; set; }
    }

    public class AdminOrderItemViewModel
    {
        public string ProductName { get; set; }

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal TotalPrice { get; set; }
    }
}