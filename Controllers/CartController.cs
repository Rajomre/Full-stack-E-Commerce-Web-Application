using Shoping_webapplication_project_.Data;
using Shoping_webapplication_project_.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Shoping_webapplication_project_.Controllers
{
    public class CartController : Controller
    {
        private ShoppingDbContext db = new ShoppingDbContext();

        // Helper to load and sync cart from SQL Database
        private List<CartItem> GetCart(int userId)
        {
            var dbItems = (from c in db.CartItems
                           join p in db.Products on c.ProductId equals p.ProductId
                           where c.UserId == userId && p.IsActive
                           select new CartItem
                           {
                               ProductId = p.ProductId,
                               ProductName = p.ProductName,
                               Price = p.Price,
                               Quantity = c.Quantity,
                               ImageUrl = p.ImageUrl,
                               StockQuantity = p.StockQuantity
                           }).ToList();

            Session["Cart"] = dbItems;
            return dbItems;
        }

        // GET: Cart
        public ActionResult Index()
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int userId = Convert.ToInt32(Session["UserId"]);
            var cart = GetCart(userId);
            return View(cart);
        }

        // POST: Cart/AddToCart
        [HttpPost]
        public ActionResult AddToCart(int productId, int quantity = 1)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int userId = Convert.ToInt32(Session["UserId"]);
            var product = db.Products.FirstOrDefault(p => p.ProductId == productId && p.IsActive);
            if (product == null || product.StockQuantity <= 0)
            {
                return RedirectToAction("Index", "Product");
            }

            var existingDbItem = db.CartItems.FirstOrDefault(c => c.UserId == userId && c.ProductId == productId);
            if (existingDbItem != null)
            {
                if (existingDbItem.Quantity + quantity <= product.StockQuantity)
                {
                    existingDbItem.Quantity += quantity;
                }
                else
                {
                    existingDbItem.Quantity = product.StockQuantity;
                }
            }
            else
            {
                int initialQty = Math.Min(quantity, product.StockQuantity);
                db.CartItems.Add(new DbCartItem
                {
                    UserId = userId,
                    ProductId = productId,
                    Quantity = initialQty,
                    CreatedAt = DateTime.Now
                });
            }

            db.SaveChanges();
            GetCart(userId); // Refresh Session

            return RedirectToAction("Index");
        }

        // POST: Cart/UpdateQuantity
        [HttpPost]
        public ActionResult UpdateQuantity(int productId, string actionType)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int userId = Convert.ToInt32(Session["UserId"]);
            var existingDbItem = db.CartItems.FirstOrDefault(c => c.UserId == userId && c.ProductId == productId);

            if (existingDbItem != null)
            {
                var product = db.Products.Find(productId);
                if (actionType == "increase")
                {
                    if (product != null && existingDbItem.Quantity < product.StockQuantity)
                    {
                        existingDbItem.Quantity++;
                    }
                }
                else if (actionType == "decrease")
                {
                    existingDbItem.Quantity--;
                    if (existingDbItem.Quantity <= 0)
                    {
                        db.CartItems.Remove(existingDbItem);
                    }
                }
                db.SaveChanges();
            }

            GetCart(userId);
            return RedirectToAction("Index");
        }

        // POST: Cart/Remove
        [HttpPost]
        public ActionResult Remove(int productId)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int userId = Convert.ToInt32(Session["UserId"]);
            var existingDbItem = db.CartItems.FirstOrDefault(c => c.UserId == userId && c.ProductId == productId);
            if (existingDbItem != null)
            {
                db.CartItems.Remove(existingDbItem);
                db.SaveChanges();
            }

            GetCart(userId);
            return RedirectToAction("Index");
        }

        // POST: Cart/Clear
        [HttpPost]
        public ActionResult Clear()
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int userId = Convert.ToInt32(Session["UserId"]);
            var userCartItems = db.CartItems.Where(c => c.UserId == userId);
            db.CartItems.RemoveRange(userCartItems);
            db.SaveChanges();

            Session["Cart"] = new List<CartItem>();
            return RedirectToAction("Index");
        }

        // GET: Cart/Checkout
        public ActionResult Checkout()
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int userId = Convert.ToInt32(Session["UserId"]);
            var cart = GetCart(userId);
            if (!cart.Any())
            {
                return RedirectToAction("Index");
            }

            var user = db.Users.Find(userId);
            ViewBag.CustomerName = user != null ? user.FullName : Convert.ToString(Session["FullName"]);
            ViewBag.CustomerEmail = user != null ? user.Email : "";
            ViewBag.CustomerPhone = user != null ? user.Phone : "";

            return View(cart);
        }

        // POST: Cart/PlaceOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PlaceOrder(string shippingAddress, string paymentMethod)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int userId = Convert.ToInt32(Session["UserId"]);
            var cart = GetCart(userId);
            if (!cart.Any())
            {
                return RedirectToAction("Index");
            }

            decimal totalAmount = cart.Sum(i => i.TotalPrice);

            // 1. Save Order
            var order = new Order
            {
                UserId = userId,
                OrderNumber = "ORD-" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                TotalAmount = totalAmount,
                Status = "Pending",
                OrderDate = DateTime.Now
            };

            db.Orders.Add(order);
            db.SaveChanges();

            // 2. Save OrderItems & reduce product stock
            foreach (var item in cart)
            {
                var orderItem = new OrderItem
                {
                    OrderId = order.OrderId,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.Price
                };
                db.OrderItems.Add(orderItem);

                var product = db.Products.Find(item.ProductId);
                if (product != null)
                {
                    product.StockQuantity = Math.Max(0, product.StockQuantity - item.Quantity);
                }
            }

            // 3. Clear database cart for this user
            var userCartItems = db.CartItems.Where(c => c.UserId == userId);
            db.CartItems.RemoveRange(userCartItems);
            db.SaveChanges();

            Session["Cart"] = new List<CartItem>();

            return RedirectToAction("OrderSuccess", new { id = order.OrderId });
        }

        // GET: Cart/OrderSuccess/5
        public ActionResult OrderSuccess(int id)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var order = db.Orders.FirstOrDefault(o => o.OrderId == id);
            if (order == null)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            return View(order);
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