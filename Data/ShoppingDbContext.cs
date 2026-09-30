using Shoping_webapplication_project_.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Shoping_webapplication_project_.Data
{
   public class ShoppingDbContext : DbContext
    {
        public ShoppingDbContext()
            : base("name=ShoppingDBConnection")
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<DbCartItem> CartItems { get; set; }
        public DbSet<DbWishlistItem> WishlistItems { get; set; }

        public DbSet<OrderItem> OrderItems { get; set; }
    }
}
