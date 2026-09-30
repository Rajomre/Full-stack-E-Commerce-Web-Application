using Shoping_webapplication_project_.Data;
using Shoping_webapplication_project_.Models;
using Shoping_webapplication_project_.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Shoping_webapplication_project_.Controllers
{

    public class AccountController : Controller
    {
        private readonly ShoppingDbContext db = new ShoppingDbContext();

        // GET: Account
        public ActionResult Registration()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Registration(RegisterViewModel model)
        {

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.Email = model.Email.Trim().ToLower();

             using (var db = new ShoppingDbContext())
            {
                var emailExists = db.Users.Any(u => u.Email == model.Email);

                if (emailExists)
                {
                    ModelState.AddModelError(
                        "Email",
                        "This email is already registered."
                    );

                    return View(model);
                }

               
            }

            var user = new User
            {

                FullName = model.FullName,
                Email = model.Email,
                Phone = model.Phone,

                // Temporary placeholder
                PasswordHash = PasswordHasher.HashPassword(model.Password),
                CreatedAt = DateTime.Now,
                Role = "User"
            };

            db.Users.Add(user);
            db.SaveChanges();

            return RedirectToAction("Login");
        }

        
        // GET: Account/Login
        public ActionResult Login()
        {
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = db.Users.FirstOrDefault(u => u.Email == model.Email);

            if (user == null ||
                !PasswordHasher.VerifyPassword(model.Password, user.PasswordHash))
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View(model);
            }

            // Check account status
            if (!user.IsActive)
            {
                ModelState.AddModelError(
                    "",
                    "Your account is inactive. Please contact the administrator."
                );

                return View(model);
            }

            Session["UserId"] = user.UserId;
            Session["FullName"] = user.FullName;
            Session["Role"] = user.Role;
            // Load user's saved Cart & Wishlist from SQL Database into Session
            Session["Cart"] = (from c in db.CartItems
                               join p in db.Products on c.ProductId equals p.ProductId
                               where c.UserId == user.UserId && p.IsActive
                               select new CartItem
                               {
                                   ProductId = p.ProductId,
                                   ProductName = p.ProductName,
                                   Price = p.Price,
                                   Quantity = c.Quantity,
                                   ImageUrl = p.ImageUrl,
                                   StockQuantity = p.StockQuantity
                               }).ToList();

            Session["Wishlist"] = (from w in db.WishlistItems
                                   join p in db.Products on w.ProductId equals p.ProductId
                                   where w.UserId == user.UserId && p.IsActive
                                   select new CartItem
                                   {
                                       ProductId = p.ProductId,
                                       ProductName = p.ProductName,
                                       Price = p.Price,
                                       Quantity = 1,
                                       ImageUrl = p.ImageUrl,
                                       StockQuantity = p.StockQuantity
                                   }).ToList();

            if (user.Role == "Admin")
            {
                return RedirectToAction("Index", "Admin");
            }

            return RedirectToAction("Index", "Dashboard");
        }
        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();

            return RedirectToAction("Login", "Account");
        }
        // GET: Account/ForgotPassword
        public ActionResult ForgotPassword()
        {
            return View();
        }

        // POST: Account/ForgotPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string email = model.Email.Trim().ToLower();
            string phone = model.Phone.Trim();

            var user = db.Users.FirstOrDefault(u => u.Email == email && u.Phone == phone);

            if (user == null)
            {
                ModelState.AddModelError("", "No account found matching this Email and Phone number combination.");
                return View(model);
            }

            user.PasswordHash = PasswordHasher.HashPassword(model.NewPassword);
            db.SaveChanges();

            TempData["SuccessMessage"] = "Your password has been reset successfully! Please login with your new password.";
            return RedirectToAction("Login");
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
    
