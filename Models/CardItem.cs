using System;

namespace Shoping_webapplication_project_.Models
{
    public class CartItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string ImageUrl { get; set; }
        public int StockQuantity { get; set; }

        public decimal TotalPrice
        {
            get { return Price * Quantity; }
        }
    }
}