using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Shoping_webapplication_project_.ViewModels
{
    public class AdminReportsViewModel
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public int TotalOrders { get; set; }
        public int TotalItemsSold { get; set; }

        public decimal TotalRevenue { get; set; }

        public int DeliveredOrders { get; set; }
        public int PendingOrders { get; set; }
        public int ProcessingOrders { get; set; }
        public int ShippedOrders { get; set; }

        public List<AdminReportOrderViewModel> Orders { get; set; }
        public List<MonthlySalesViewModel> MonthlySales { get; set; }
    }

    public class AdminReportOrderViewModel
    {
        public string OrderNumber { get; set; }
        public string CustomerName { get; set; }
        public int TotalItems { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public DateTime OrderDate { get; set; }
    }
}