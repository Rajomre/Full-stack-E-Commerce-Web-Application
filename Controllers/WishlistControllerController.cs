using Shoping_webapplication_project_.Data;
using Shoping_webapplication_project_.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Shoping_webapplication_project_.Controllers
{
    public class WishlistController : Controller
    {
        private ShoppingDbContext db = new ShoppingDbContext();

        // Helper to load and sync Wishlist from SQL Database
        private List<CartItem> GetWishlist(int userId)
        {
            var dbItems = (from w in db.WishlistItems
                           join p in db.Products on w.ProductId equals p.ProductId
                           where w.UserId == userId && p.IsActive
                           select new CartItem
                           {
                               ProductId = p.ProductId,
                               ProductName = p.ProductName,
                               Price = p.Price,
                               Quantity = 1,
                               ImageUrl = p.ImageUrl,
                               StockQuantity = p.StockQuantity
                           }).ToList();

            Session["Wishlist"] = dbItems;
            return dbItems;
        }

        // GET: Wishlist
        public ActionResult Index()
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int userId = Convert.ToInt32(Session["UserId"]);
            var wishlist = GetWishlist(userId);
            return View(wishlist);
        }

        // POST: Wishlist/Add
        [HttpPost]
        public ActionResult Add(int productId)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int userId = Convert.ToInt32(Session["UserId"]);
            var product = db.Products.FirstOrDefault(p => p.ProductId == productId && p.IsActive);
            if (product != null)
            {
                var existing = db.WishlistItems.FirstOrDefault(w => w.UserId == userId && w.ProductId == productId);
                if (existing == null)
                {
                    db.WishlistItems.Add(new DbWishlistItem
                    {
                        UserId = userId,
                        ProductId = productId,
                        CreatedAt = DateTime.Now
                    });
                    db.SaveChanges();
                }
            }

            GetWishlist(userId); // Refresh Session

            if (Request.UrlReferrer != null)
            {
                return Redirect(Request.UrlReferrer.ToString());
            }
            return RedirectToAction("Index");
        }

        // POST: Wishlist/Remove
        [HttpPost]
        public ActionResult Remove(int productId)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int userId = Convert.ToInt32(Session["UserId"]);
            var existing = db.WishlistItems.FirstOrDefault(w => w.UserId == userId && w.ProductId == productId);
            if (existing != null)
            {
                db.WishlistItems.Remove(existing);
                db.SaveChanges();
            }

            GetWishlist(userId);
            return RedirectToAction("Index");
        }

        // POST: Wishlist/MoveToCart
        [HttpPost]
        public ActionResult MoveToCart(int productId)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int userId = Convert.ToInt32(Session["UserId"]);

            // 1. Remove from wishlist
            var wishlistRecord = db.WishlistItems.FirstOrDefault(w => w.UserId == userId && w.ProductId == productId);
            if (wishlistRecord != null)
            {
                db.WishlistItems.Remove(wishlistRecord);
            }

            // 2. Add to database Cart
            var product = db.Products.Find(productId);
            if (product != null && product.StockQuantity > 0)
            {
                var cartRecord = db.CartItems.FirstOrDefault(c => c.UserId == userId && c.ProductId == productId);
                if (cartRecord != null)
                {
                    cartRecord.Quantity++;
                }
                else
                {
                    db.CartItems.Add(new DbCartItem
                    {
                        UserId = userId,
                        ProductId = productId,
                        Quantity = 1,
                        CreatedAt = DateTime.Now
                    });
                }
            }

            db.SaveChanges();

            // Refresh both sessions
            GetWishlist(userId);

            return RedirectToAction("Index", "Cart");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}