using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Shoping_webapplication_project_.ViewModels
{
    
        public class MonthlySalesViewModel
        {
            public string Month { get; set; }

            public int TotalOrders { get; set; }

            public int TotalItemsSold { get; set; }

            public decimal TotalRevenue { get; set; }
        }
    }
