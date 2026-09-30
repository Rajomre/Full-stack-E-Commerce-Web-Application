using Shoping_webapplication_project_.Data;
using Shoping_webapplication_project_.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Shoping_webapplication_project_.Controllers
{
    public class DashboardController : Controller
    {
        private ShoppingDbContext db = new ShoppingDbContext();

        // GET: Dashboard
        // GET: Dashboard
        public ActionResult Index()
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.TotalRevenue = db.Orders
                                     .Where(o => o.Status != "Cancelled")
                                     .Sum(o => (decimal?)o.TotalAmount) ?? 0;

            ViewBag.RealCategories = db.Products
                                       .Where(p => p.IsActive && !string.IsNullOrEmpty(p.Category))
                                       .Select(p => p.Category)
                                       .Distinct()
                                       .Take(6)
                                       .ToList();

            var featuredProducts = db.Products
                                     .Where(p => p.IsActive)
                                     .OrderByDescending(p => p.ProductId)
                                     .Take(4)
                                     .ToList();

            return View(featuredProducts);
        }

        // GET: Dashboard/Orders
        public ActionResult Orders()
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int userId = Convert.ToInt32(Session["UserId"]);

            var myOrders = db.Orders
                             .Where(o => o.UserId == userId)
                             .OrderByDescending(o => o.OrderDate)
                             .ToList();

            return View(myOrders);
        }

        // GET: Dashboard/OrderDetails/5
        public ActionResult OrderDetails(int? id)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            int userId = Convert.ToInt32(Session["UserId"]);

            var order = db.Orders.FirstOrDefault(o => o.OrderId == id && o.UserId == userId);
            if (order == null)
            {
                return HttpNotFound();
            }

            var orderItems = (from oi in db.OrderItems
                              join p in db.Products on oi.ProductId equals p.ProductId
                              where oi.OrderId == id
                              select new UserOrderItemViewModel
                              {
                                  ProductId = p.ProductId,
                                  ProductName = p.ProductName,
                                  ImageUrl = p.ImageUrl,
                                  Quantity = oi.Quantity,
                                  UnitPrice = oi.UnitPrice,
                                  TotalPrice = oi.Quantity * oi.UnitPrice
                              }).ToList();

            var viewModel = new UserOrderDetailsViewModel
            {
                Order = order,
                Items = orderItems
            };

            return View(viewModel);
        }

        // POST: Dashboard/CancelOrder/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CancelOrder(int id)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int userId = Convert.ToInt32(Session["UserId"]);

            var order = db.Orders.FirstOrDefault(o => o.OrderId == id && o.UserId == userId);

            if (order != null && order.Status == "Pending")
            {
                order.Status = "Cancelled";

                var orderItems = db.OrderItems.Where(oi => oi.OrderId == id).ToList();
                foreach (var item in orderItems)
                {
                    var product = db.Products.Find(item.ProductId);
                    if (product != null)
                    {
                        product.StockQuantity += item.Quantity; 
                    }
                }

                db.SaveChanges();
                TempData["OrderCancelMessage"] = "Your order has been successfully cancelled and stock has been restored.";
            }

            return RedirectToAction("OrderDetails", new { id = id });
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        // GET: Dashboard/Profile
        public new ActionResult  Profile()
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int userId = Convert.ToInt32(Session["UserId"]);
            var user = db.Users.Find(userId);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return View(user);
        }

        // POST: Dashboard/UpdateProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateProfile(string fullName, string phone)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int userId = Convert.ToInt32(Session["UserId"]);
            var user = db.Users.Find(userId);

            if (user != null)
            {
                if (!string.IsNullOrWhiteSpace(fullName))
                {
                    user.FullName = fullName.Trim();
                    user.Phone = !string.IsNullOrWhiteSpace(phone) ? phone.Trim() : "";

                    db.SaveChanges();

                    Session["FullName"] = user.FullName;

                    TempData["ProfileSuccess"] = "Profile details updated successfully!";
                }
                else
                {
                    TempData["ProfileError"] = "Full Name cannot be left blank.";
                }
            }

            return RedirectToAction("Profile");
        }

        // POST: Dashboard/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (string.IsNullOrWhiteSpace(currentPassword) || string.IsNullOrWhiteSpace(newPassword))
            {
                TempData["PasswordError"] = "Please fill in all password fields.";
                return RedirectToAction("Profile");
            }

            if (newPassword != confirmPassword)
            {
                TempData["PasswordError"] = "New Password and Confirm Password do not match.";
                return RedirectToAction("Profile");
            }

            if (newPassword.Length < 6)
            {
                TempData["PasswordError"] = "New Password must be at least 6 characters long.";
                return RedirectToAction("Profile");
            }

            int userId = Convert.ToInt32(Session["UserId"]);
            var user = db.Users.Find(userId);

            if (user != null)
            {
                bool isCurrentValid = PasswordHasher.VerifyPassword(currentPassword, user.PasswordHash);
                if (!isCurrentValid)
                {
                    TempData["PasswordError"] = "Current password is incorrect.";
                    return RedirectToAction("Profile");
                }

                user.PasswordHash = PasswordHasher.HashPassword(newPassword);
                db.SaveChanges();

                TempData["PasswordSuccess"] = "Password has been changed successfully!";
            }

            return RedirectToAction("Profile");
        }
    }
}