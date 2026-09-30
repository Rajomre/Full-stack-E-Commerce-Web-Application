using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Shoping_webapplication_project_.ViewModels
{
    public class AdminOrderViewModel
    {
        public string OrderId { get; set; }
        public int DatabaseOrderId { get; set; }

        public string CustomerName { get; set; }

        public int TotalItems { get; set; }

        public decimal TotalAmount { get; set; }

        public string Status { get; set; }

        public DateTime OrderDate { get; set; }

    }
}