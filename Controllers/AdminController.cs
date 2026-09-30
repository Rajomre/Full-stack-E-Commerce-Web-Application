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
    public class AdminController : Controller
    {
        // GET: Admin
        public ActionResult Index()
        {
            var db = new ShoppingDbContext();
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (Session["Role"]?.ToString() != "Admin")
            {
                return RedirectToAction("Index", "Dashboard");
            }
            ViewBag.TotalOrders = db.Orders.Count(o => o.Status != "Cancelled");

            ViewBag.TotalRevenue = db.Orders
                                     .Where(o => o.Status != "Cancelled")
                                     .Sum(o => (decimal?)o.TotalAmount) ?? 0;
            using (var d = new ShoppingDbContext())
            {
                ViewBag.TotalUsers = d.Users.Count();
                ViewBag.TotalProducts = d.Products.Count();
                ViewBag.TotalOrders = d.Orders.Count(o => o.Status != "Cancelled");

                ViewBag.TotalRevenue = d.Orders
                                         .Where(o => o.Status != "Cancelled")
                                         .Sum(o => (decimal?)o.TotalAmount) ?? 0;
                // Latest 5 real orders
                var recentOrders = d.Orders
                    .Include("User")
                    .Include("OrderItems")
                    .OrderByDescending(o => o.OrderDate)
                    .Take(5)
                    .ToList()
                    .Select(o => new AdminOrderViewModel
                    {
                        OrderId = o.OrderNumber,
                        DatabaseOrderId = o.OrderId,
                        CustomerName = o.User.FullName,
                        TotalItems = o.OrderItems.Sum(i => i.Quantity),
                        TotalAmount = o.TotalAmount,
                        Status = o.Status,
                        OrderDate = o.OrderDate
                    })
                    .ToList();

                ViewBag.RecentOrders = recentOrders;
            }

            return View();
        }



        public ActionResult Users(string search, string role, int page = 1)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (Session["Role"]?.ToString() != "Admin")
            {
                return RedirectToAction("Index", "Dashboard");
            }

            const int pageSize = 10;

            if (page < 1)
            {
                page = 1;
            }

            using (var db = new ShoppingDbContext())
            {
                var query = db.Users.AsQueryable();

                // Search
                if (!string.IsNullOrWhiteSpace(search))
                {
                    search = search.Trim();

                    query = query.Where(u =>
                        u.FullName.Contains(search) ||
                        u.Email.Contains(search) ||
                        u.Phone.Contains(search));
                }

                // Role filter
                if (!string.IsNullOrWhiteSpace(role) && role != "All")
                {
                    query = query.Where(u => u.Role == role);
                }

                // Total users after search/filter
                var totalUsers = query.Count();

                // Calculate total pages
                var totalPages = (int)Math.Ceiling(
                    (double)totalUsers / pageSize
                );

                // If requested page is greater than available pages
                if (totalPages > 0 && page > totalPages)
                {
                    page = totalPages;
                }

                // Get only users for current page
                var users = query
                    .OrderByDescending(u => u.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var model = new AdminUsersViewModel
                {
                    Users = users,
                    Search = search,
                    Role = role,

                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalPages = totalPages,
                    TotalUsers = totalUsers
                };

                return View(model);
            }
        }
        /// for Products page
        public ActionResult Products(string search, string category)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (Session["Role"]?.ToString() != "Admin")
            {
                return RedirectToAction("Index", "Dashboard");
            }

            using (var db = new ShoppingDbContext())
            {
                var query = db.Products.AsQueryable();

                if (!string.IsNullOrWhiteSpace(search))
                {
                    search = search.Trim();

                    query = query.Where(p =>
                        p.ProductName.Contains(search) ||
                        p.Category.Contains(search));
                }

                if (!string.IsNullOrWhiteSpace(category) &&
                    category != "All")
                {
                    query = query.Where(p => p.Category == category);
                }

                var products = query
                    .OrderByDescending(p => p.CreatedAt)
                    .ToList();

                ViewBag.Search = search;
                ViewBag.Category = category;

                ViewBag.Categories = db.Products
                    .Select(p => p.Category)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToList();

                return View(products);
            }
        }

        [HttpGet]
        public ActionResult AddProduct()
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (Session["Role"]?.ToString() != "Admin")
            {
                return RedirectToAction("Index", "Dashboard");
            }

            var model = new AdminAddProductViewModel
            {
                IsActive = true
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddProduct(AdminAddProductViewModel model)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (Session["Role"]?.ToString() != "Admin")
            {
                return RedirectToAction("Index", "Dashboard");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (var db = new ShoppingDbContext())
            {
                var product = new Product
                {
                    ProductName = model.ProductName.Trim(),
                    Description = model.Description.Trim(),
                    Price = model.Price,
                    StockQuantity = model.StockQuantity,
                    Category = model.Category.Trim(),
                    ImageUrl = string.IsNullOrWhiteSpace(model.ImageUrl)
                        ? null
                        : model.ImageUrl.Trim(),
                    IsActive = model.IsActive,
                    CreatedAt = DateTime.Now
                };

                db.Products.Add(product);
                db.SaveChanges();
            }

            TempData["Success"] = "Product added successfully.";

            return RedirectToAction("Products");
        }

        public ActionResult ProductDetails(int id)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (Session["Role"]?.ToString() != "Admin")
            {
                return RedirectToAction("Index", "Dashboard");
            }

            using (var db = new ShoppingDbContext())
            {
                var product = db.Products
                    .FirstOrDefault(p => p.ProductId == id);

                if (product == null)
                {
                    return HttpNotFound();
                }

                return View(product);
            }
        }
        [HttpGet]
        public ActionResult EditProduct(int id)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (Session["Role"]?.ToString() != "Admin")
            {
                return RedirectToAction("Index", "Dashboard");
            }

            using (var db = new ShoppingDbContext())
            {
                var product = db.Products.FirstOrDefault(p => p.ProductId == id);

                if (product == null)
                {
                    return HttpNotFound();
                }

                var model = new AdminEditProductViewModel
                {
                    ProductId = product.ProductId,
                    ProductName = product.ProductName,
                    Description = product.Description,
                    Price = product.Price,
                    StockQuantity = product.StockQuantity,
                    Category = product.Category,
                    ImageUrl = product.ImageUrl,
                    IsActive = product.IsActive
                };

                return View(model);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditProduct(AdminEditProductViewModel model)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (Session["Role"]?.ToString() != "Admin")
            {
                return RedirectToAction("Index", "Dashboard");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (var db = new ShoppingDbContext())
            {
                var product = db.Products
                    .FirstOrDefault(p => p.ProductId == model.ProductId);

                if (product == null)
                {
                    return HttpNotFound();
                }

                product.ProductName = model.ProductName.Trim();
                product.Description = model.Description.Trim();
                product.Price = model.Price;
                product.StockQuantity = model.StockQuantity;
                product.Category = model.Category.Trim();

                product.ImageUrl = string.IsNullOrWhiteSpace(model.ImageUrl)
                    ? null
                    : model.ImageUrl.Trim();

                product.IsActive = model.IsActive;

                db.SaveChanges();
            }

            TempData["Success"] = "Product updated successfully.";

            return RedirectToAction(
                "ProductDetails",
                new { id = model.ProductId }
            );
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteProduct(int id)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (Session["Role"]?.ToString() != "Admin")
            {
                return RedirectToAction("Index", "Dashboard");
            }

            using (var db = new ShoppingDbContext())
            {
                var product = db.Products
                    .FirstOrDefault(p => p.ProductId == id);

                if (product == null)
                {
                    return HttpNotFound();
                }

                db.Products.Remove(product);
                db.SaveChanges();
            }

            TempData["Success"] = "Product deleted successfully.";

            return RedirectToAction("Products");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ToggleProductStatus(int id)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (Session["Role"]?.ToString() != "Admin")
            {
                return RedirectToAction("Index", "Dashboard");
            }

            using (var db = new ShoppingDbContext())
            {
                var product = db.Products
                    .FirstOrDefault(p => p.ProductId == id);

                if (product == null)
                {
                    return HttpNotFound();
                }

                product.IsActive = !product.IsActive;

                db.SaveChanges();

                TempData["Success"] = product.IsActive
                    ? "Product activated successfully."
                    : "Product deactivated successfully.";
            }

            return RedirectToAction("ProductDetails", new { id = id });
        }
        public ActionResult Details(int id)
        {
            // Check login
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Check Admin role
            if (Session["Role"]?.ToString() != "Admin")
            {
                return RedirectToAction("Index", "Dashboard");
            }

            using (var db = new ShoppingDbContext())
            {
                var user = db.Users.FirstOrDefault(u => u.UserId == id);

                if (user == null)
                {
                    return HttpNotFound();
                }

                return View(user);
            }
        }

        public ActionResult Edit(int id)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (Session["Role"]?.ToString() != "Admin")
            {
                return RedirectToAction("Index", "Dashboard");
            }

            using (var db = new ShoppingDbContext())
            {
                var user = db.Users.FirstOrDefault(u => u.UserId == id);

                if (user == null)
                {
                    return HttpNotFound();
                }
                var model = new AdminEditUserViewModel
                {
                    UserId = user.UserId,
                    FullName = user.FullName,
                    Email = user.Email,
                    Phone = user.Phone,
                    Role = user.Role
                };

                return View(model);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(AdminEditUserViewModel model)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (Session["Role"]?.ToString() != "Admin")
            {
                return RedirectToAction("Index", "Dashboard");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (var db = new ShoppingDbContext())
            {
                var user = db.Users.FirstOrDefault(u => u.UserId == model.UserId);

                if (user == null)
                {
                    return HttpNotFound();
                }

                model.Email = model.Email.Trim().ToLower();

                var emailExists = db.Users.Any(u =>
                    u.Email == model.Email &&
                    u.UserId != model.UserId);
                if (emailExists)
                {
                    ModelState.AddModelError(
                        "Email",
                        "This email is already registered with another user."
                    );

                    return View(model);
                }

                // Prevent Admin from removing own Admin role
                if (user.UserId == Convert.ToInt32(Session["UserId"]) &&
                    string.Equals(user.Role, "Admin", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(model.Role, "Admin", StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError(
                        "Role",
                        "You cannot remove the Admin role from your own account."
                    );

                    return View(model);
                }

                user.FullName = model.FullName;
                user.Email = model.Email;
                user.Phone = model.Phone;
                user.Role = model.Role;

                db.SaveChanges();

                return RedirectToAction("Details", new { id = user.UserId });
            }
       
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ToggleUserStatus(int id)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (Session["Role"]?.ToString() != "Admin")
            {
                return RedirectToAction("Index", "Dashboard");
            }

            using (var db = new ShoppingDbContext())
            {
                var user = db.Users.FirstOrDefault(u => u.UserId == id);

                if (user == null)
                {
                    return HttpNotFound();
                }

                // Admin apna account deactivate nahi kar sakta
                if (user.UserId == Convert.ToInt32(Session["UserId"]))
                {
                    TempData["Error"] = "You cannot deactivate your own account.";
                    return RedirectToAction("Users");
                }

                user.IsActive = !user.IsActive;

                db.SaveChanges();

                TempData["Success"] = user.IsActive
                    ? "User account activated successfully."
                    : "User account deactivated successfully.";

                return RedirectToAction("Users");
            }
        }
        public ActionResult Orders(string search, string status)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (Session["Role"]?.ToString() != "Admin")
            {
                return RedirectToAction("Index", "Dashboard");
            }

            using (var db = new ShoppingDbContext())
            {
                var query = db.Orders
                    .Include("User")
                    .Include("OrderItems")
                    .AsQueryable();

                // Search by Order Number or Customer Name
                if (!string.IsNullOrWhiteSpace(search))
                {
                    search = search.Trim();

                    query = query.Where(o =>
                        o.OrderNumber.Contains(search) ||
                        o.User.FullName.Contains(search));
                }

                // Filter by Status
                if (!string.IsNullOrWhiteSpace(status))
                {
                    query = query.Where(o => o.Status == status);
                }

                var orders = query
                    .OrderByDescending(o => o.OrderDate)
                    .ToList()
                    .Select(o => new AdminOrderViewModel
                    {
                        OrderId = o.OrderNumber,
                        DatabaseOrderId = o.OrderId,
                        CustomerName = o.User.FullName,
                        TotalItems = o.OrderItems.Sum(i => i.Quantity),
                        TotalAmount = o.TotalAmount,
                        Status = o.Status,
                        OrderDate = o.OrderDate
                    })
                    .ToList();

                return View(orders);
            }
        }

        [HttpPost]
        public ActionResult UpdateOrderStatus(int id, string status)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (Session["Role"]?.ToString() != "Admin")
            {
                return RedirectToAction("Index", "Dashboard");
            }

            using (var db = new ShoppingDbContext())
            {
                var order = db.Orders.FirstOrDefault(o => o.OrderId == id);

                if (order == null)
                {
                    return HttpNotFound();
                }

                order.Status = status;

                db.SaveChanges();
            }

            return RedirectToAction("Orders");
        }
        public ActionResult OrderDetails(int id)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (Session["Role"]?.ToString() != "Admin")
            {
                return RedirectToAction("Index", "Dashboard");
            }

            using (var db = new ShoppingDbContext())
            {
                var order = db.Orders
                    .Include("User")
                    .Include("OrderItems.Product")
                    .FirstOrDefault(o => o.OrderId == id);

                if (order == null)
                {
                    return HttpNotFound();
                }

                var model = new AdminOrderDetailsViewModel
                {
                    OrderNumber = order.OrderNumber,
                    CustomerName = order.User.FullName,
                    CustomerEmail = order.User.Email,
                    Status = order.Status,
                    OrderId = order.OrderId,

                    OrderDate = order.OrderDate,
                    TotalAmount = order.TotalAmount,

                    Items = order.OrderItems.Select(item => new AdminOrderItemViewModel
                    {
                        ProductName = item.Product.ProductName,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        TotalPrice = item.Quantity * item.UnitPrice
                    }).ToList()
                };

                return View(model);
            }
        }

        public ActionResult Revenue()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("Login", "Account");

            if (Session["Role"]?.ToString() != "Admin")
                return RedirectToAction("Index", "Dashboard");

            using (var db = new ShoppingDbContext())
            {
                ViewBag.TotalRevenue = db.Orders
                                         .Where(o => o.Status != "Cancelled")
                                         .Sum(o => (decimal?)o.TotalAmount) ?? 0;
                ViewBag.TotalOrders = db.Orders.Count();

                ViewBag.DeliveredRevenue =
                    db.Orders
                      .Where(o => o.Status == "Delivered")
                      .Sum(o => (decimal?)o.TotalAmount) ?? 0;

                ViewBag.PendingRevenue =
                    db.Orders
                      .Where(o => o.Status == "Pending")
                      .Sum(o => (decimal?)o.TotalAmount) ?? 0;

                ViewBag.ProcessingRevenue =
                    db.Orders
                      .Where(o => o.Status == "Processing")
                      .Sum(o => (decimal?)o.TotalAmount) ?? 0;

                var revenueOrders = db.Orders
                    .Include("User")
                    .OrderByDescending(o => o.OrderDate)
                    .ToList();

                return View(revenueOrders);
            }
        }

        public ActionResult Reports(DateTime? fromDate, DateTime? toDate)
{
    if (Session["UserId"] == null)
        return RedirectToAction("Login", "Account");

    if (Session["Role"]?.ToString() != "Admin")
        return RedirectToAction("Index", "Dashboard");

    using (var db = new ShoppingDbContext())
    {
        var query = db.Orders
            .Include("User")
            .Include("OrderItems")
            .AsQueryable();

        // Date filter
        if (fromDate.HasValue)
        {
            var startDate = fromDate.Value.Date;
            query = query.Where(o => o.OrderDate >= startDate);
        }

        if (toDate.HasValue)
        {
            var endDate = toDate.Value.Date.AddDays(1);
            query = query.Where(o => o.OrderDate < endDate);
        }

        var orders = query
            .OrderByDescending(o => o.OrderDate)
            .ToList();
                var monthlySales = orders
    .GroupBy(o => new
    {
        o.OrderDate.Year,
        o.OrderDate.Month
    })
    .OrderByDescending(g => g.Key.Year)
    .ThenByDescending(g => g.Key.Month)
    .Select(g => new MonthlySalesViewModel
    {
        Month = new DateTime(
            g.Key.Year,
            g.Key.Month,
            1
        ).ToString("MMMM yyyy"),

        TotalOrders = g.Count(),

        TotalItemsSold = g
            .SelectMany(o => o.OrderItems)
            .Sum(i => i.Quantity),

TotalRevenue = orders.Where(o => o.Status != "Cancelled").Sum(o => o.TotalAmount),

  

      })
    .ToList();

                var model = new AdminReportsViewModel
                {
                    FromDate = fromDate,
                    ToDate = toDate,

                    TotalOrders = orders.Count,

                    TotalItemsSold = orders
                  .SelectMany(o => o.OrderItems)
                  .Sum(i => i.Quantity),

                    TotalRevenue = orders.Where(o => o.Status != "Cancelled").Sum(o => o.TotalAmount),

                    DeliveredOrders = orders.Count(o => o.Status == "Delivered"),
                    PendingOrders = orders.Count(o => o.Status == "Pending"),
                    ProcessingOrders = orders.Count(o => o.Status == "Processing"),
                    ShippedOrders = orders.Count(o => o.Status == "Shipped"),

                    Orders = orders.Select(o => new AdminReportOrderViewModel
                    {
                        OrderNumber = o.OrderNumber,
                        CustomerName = o.User.FullName,
                        TotalItems = o.OrderItems.Sum(i => i.Quantity),
                        TotalAmount = o.TotalAmount,
                        Status = o.Status,
                        OrderDate = o.OrderDate
                    }).ToList(),

                    MonthlySales = monthlySales
                };

                return View(model);
    }
}

        public ActionResult Settings()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("Login", "Account");

            if (Session["Role"]?.ToString() != "Admin")
                return RedirectToAction("Index", "Dashboard");

            int userId = Convert.ToInt32(Session["UserId"]);

            using (var db = new ShoppingDbContext())
            {
                var admin = db.Users.FirstOrDefault(u => u.UserId == userId);

                if (admin == null)
                    return HttpNotFound();

                var model = new AdminSettingsViewModel
                {
                    UserId = admin.UserId,
                    FullName = admin.FullName,
                    Email = admin.Email,
                    Phone = admin.Phone,
                    Role = admin.Role,
                    IsActive = admin.IsActive
                };

                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateSettings(AdminSettingsViewModel model)
        {
            if (Session["UserId"] == null)
                return RedirectToAction("Login", "Account");

            if (Session["Role"]?.ToString() != "Admin")
                return RedirectToAction("Index", "Dashboard");

            if (!ModelState.IsValid)
                return View("Settings", model);

            int userId = Convert.ToInt32(Session["UserId"]);

            using (var db = new ShoppingDbContext())
            {
                var admin = db.Users.FirstOrDefault(u => u.UserId == userId);

                if (admin == null)
                    return HttpNotFound();

                // Update profile information
                admin.FullName = model.FullName;
                admin.Email = model.Email;
                admin.Phone = model.Phone;

                db.SaveChanges();

                // Update session name also
                Session["FullName"] = admin.FullName;
            }

            TempData["SuccessMessage"] = "Profile settings updated successfully.";

            return RedirectToAction("Settings");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(AdminSettingsViewModel model)
        {
            if (Session["UserId"] == null)
                return RedirectToAction("Login", "Account");

            if (Session["Role"]?.ToString() != "Admin")
                return RedirectToAction("Index", "Dashboard");

            if (string.IsNullOrWhiteSpace(model.CurrentPassword) ||
                string.IsNullOrWhiteSpace(model.NewPassword) ||
                string.IsNullOrWhiteSpace(model.ConfirmPassword))
            {
                TempData["PasswordError"] = "Please fill all password fields.";
                return RedirectToAction("Settings");
            }

            if (model.NewPassword != model.ConfirmPassword)
            {
                TempData["PasswordError"] = "New password and confirm password do not match.";
                return RedirectToAction("Settings");
            }

            int userId = Convert.ToInt32(Session["UserId"]);

            using (var db = new ShoppingDbContext())
            {
                var admin = db.Users.FirstOrDefault(u => u.UserId == userId);

                if (admin == null)
                    return HttpNotFound();

                // Verify existing password using the existing PasswordHasher
                if (!PasswordHasher.VerifyPassword(model.CurrentPassword, admin.PasswordHash))
                {
                    TempData["PasswordError"] = "Current password is incorrect.";
                    return RedirectToAction("Settings");
                }

                // Keep the existing password hashing system
                admin.PasswordHash = PasswordHasher.HashPassword(model.NewPassword);

                db.SaveChanges();
            }

            TempData["PasswordSuccess"] = "Password changed successfully.";

            return RedirectToAction("Settings");
        }

        public ActionResult HelpSupport()
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (Session["Role"]?.ToString() != "Admin")
            {
                return RedirectToAction("Index", "Dashboard");
            }

            return View();
        }
    }
    }
