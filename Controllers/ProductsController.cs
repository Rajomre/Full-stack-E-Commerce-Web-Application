using Shoping_webapplication_project_.Data;
using Shoping_webapplication_project_.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Shoping_webapplication_project_.Controllers
{
    public class ProductController : Controller
    {
        private ShoppingDbContext db = new ShoppingDbContext();

        // GET: Product (User Catalog)
        public ActionResult Index(string search, string category)
        {
            // Session check - ensure user is logged in
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Only fetch active products from database
            var productsQuery = db.Products.Where(p => p.IsActive);

            // Filter by search term if provided
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                productsQuery = productsQuery.Where(p => p.ProductName.Contains(search) ||
                                                        p.Category.Contains(search));
                ViewBag.CurrentSearch = search;
            }

            if (!string.IsNullOrWhiteSpace(category) && category != "All")
            {
                string trimmedCategory = category.Trim().ToLower();
                productsQuery = productsQuery.Where(p => p.Category.Trim().ToLower() == trimmedCategory);
                ViewBag.CurrentCategory = category;
            }
            else
            {
                ViewBag.CurrentCategory = "All";
            }

            // Get distinct categories from active products for filter buttons
            ViewBag.Categories = db.Products
                                   .Where(p => p.IsActive && !string.IsNullOrEmpty(p.Category))
                                   .Select(p => p.Category)
                                   .Distinct()
                                   .ToList();

            var products = productsQuery.OrderByDescending(p => p.ProductId).ToList();

            return View(products);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        // GET: Product/Details/5
        public ActionResult Details(int? id)
        {
            // Session check
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            // Find active product
            var product = db.Products.FirstOrDefault(p => p.ProductId == id && p.IsActive);
            if (product == null)
            {
                return HttpNotFound();
            }

            // Fetch related products from same category (excluding current product)
            ViewBag.RelatedProducts = db.Products
                                        .Where(p => p.IsActive &&
                                                    p.Category == product.Category &&
                                                    p.ProductId != product.ProductId)
                                        .Take(4)
                                        .ToList();

            return View(product);
        }
    }
}