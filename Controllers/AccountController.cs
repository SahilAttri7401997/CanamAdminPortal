﻿using CanamDistributors.Interfaces;
using CanamDistributors.Models;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text;

namespace CanamDistributors.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IQuickBookService _quickBookService;
        public AccountController(IAuthService authService, IQuickBookService quickBookService)
        {
            _authService = authService;
            _quickBookService = quickBookService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            var erroMessage = TempData["ErrorMessage"] as string;
            if (!string.IsNullOrEmpty(erroMessage))
            {
                ViewBag.ErrorMessage = erroMessage;
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LogInViewModel model)
        {
            if (ModelState.IsValid)
            {
                var admin = await _authService.ValidateUserAsync(model);
                if (admin != null)
                {
                    // Store admin details in session
                    HttpContext.Session.SetString("AdminName", admin.Name); // Adjust property names as needed
                    //string redirectURL = _quickBookService.InitiateAuthQuickBook();
                    //if (redirectURL != null)
                    //{
                    //    return Redirect(redirectURL);
                    //}
                    return RedirectToAction("DashBoard", "Account");
                }
                else
                {
                    ModelState.AddModelError("", "Invalid email or password.");
                }
            }
            return View(model);
        }

        // Dashboard method
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            // Retrieve admin details from session
            var adminName = HttpContext.Session.GetString("AdminName");
            if (adminName == null)
            {
                TempData["ErrorMessage"] = "401 UnAuthorized Please login first.";
                return RedirectToAction("Login");
            }
            ViewBag.AdminName = adminName;
            return View();
        }

        public async Task<IActionResult> Callback()
        {
            var code = Request.Query["code"].ToString();
            var realMId = Request.Query["RealmId"].ToString();
            var tokenResponse = await _quickBookService.GetAuthTokensAsync(code);
            HttpContext.Session.SetString("accessToken", tokenResponse.AccessToken);
            HttpContext.Session.SetString("refreshToken", tokenResponse.RefreshToken);
            HttpContext.Session.SetString("realMId", realMId);
            return RedirectToAction("GetInventory");
        }

        public async Task<IActionResult> GetInventory()
        {
            var accessToken = HttpContext.Session.GetString("accessToken");
            var staticRealmId = HttpContext.Session.GetString("realMId");
            await _quickBookService.UpdateInventoryItemsAsync(accessToken, staticRealmId);
            return RedirectToAction("Dashboard");

        }

        [HttpGet]
        public async Task<IActionResult> Products()
        {
            // Retrieve admin details from session
            var adminName = HttpContext.Session.GetString("AdminName");
            if (adminName == null)
            {
                TempData["ErrorMessage"] = "401 UnAuthorized Please login first.";
                return RedirectToAction("Login");
            }
            ViewBag.AdminName = adminName;
            var products = await _authService.GetProducts();
            if (products == null)
            {
                products = new List<ProductResponseModel> { new ProductResponseModel() };
            }
            return View(products);
        }


        public async Task<IActionResult> ExportProducts()
        {
            var products = await _authService.GetProducts(); // Fetch products from your service
            var csvData = _authService.GenerateCsv(products);
            return File(Encoding.UTF8.GetBytes(csvData), "text/csv", "products.csv");
        }

        public async Task<IActionResult> PrintPreview()
        {
            try
            {
                // Replace this with actual data retrieval logic
                var products = await _authService.GetProducts();

                // Create a memory stream to hold the PDF// Create a memory stream to hold the PDF
                var memoryStream = new MemoryStream();

                // Initialize PDF writer
                using (var writer = new PdfWriter(memoryStream))
                {
                    // Initialize PDF document
                    using (var pdf = new PdfDocument(writer))
                    {
                        var document = new Document(pdf);

                        // Create a table with 9 columns
                        var table = new Table(9);
                        table.AddHeaderCell("Category");
                        table.AddHeaderCell("Name");
                        table.AddHeaderCell("SKU");
                        table.AddHeaderCell("Type");
                        table.AddHeaderCell("Sales Description");
                        table.AddHeaderCell("Sales Price");
                        table.AddHeaderCell("Cost");
                        table.AddHeaderCell("Qty On Hand");
                        table.AddHeaderCell("ReOrder Point");

                        // Add records to the table
                        foreach (var category in products)
                        {
                            table.AddCell(category.ParentRefName ?? "N/A");
                            table.AddCell(category.Name ?? "N/A");
                            table.AddCell(category.Name ?? "N/A");
                            table.AddCell(category.Type ?? "N/A");
                            table.AddCell(category.Description ?? "N/A");
                            table.AddCell(category.UnitPrice.ToString()); // Format as currency
                            table.AddCell(category.PurchaseCost.ToString()); // Format as currency
                            table.AddCell(category.QtyOnHand.ToString());
                            table.AddCell(category.ReorderPoint.ToString());
                        }

                        // Add table to the document
                        document.Add(table);
                    }
                }

                // Reset stream position to the beginning

                var fileContent = memoryStream.ToArray();
                return File(fileContent, "application/pdf", "ProductsList.pdf");
            }
            catch (Exception ex)
            {
                // Handle or log the exception
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Collections()
        {
            // Retrieve admin details from session
            var adminName = HttpContext.Session.GetString("AdminName");
            if (adminName == null)
            {
                TempData["ErrorMessage"] = "401 UnAuthorized Please login first.";
                return RedirectToAction("Login");
            }
            ViewBag.AdminName = adminName;
            var collections = await _authService.GetCollections();
            if (collections == null)
            {
                collections = new List<Entity.CategoryEntity> { new Entity.CategoryEntity() };
            }
            return View(collections);
        }

        [HttpGet]
        public async Task<IActionResult> CreateCollection()
        {
            // Retrieve admin details from session
            var adminName = HttpContext.Session.GetString("AdminName");
            if (adminName == null)
            {
                TempData["ErrorMessage"] = "401 UnAuthorized Please login first.";
                return RedirectToAction("Login");
            }
            ViewBag.AdminName = adminName;
            ViewBag.ProductList = await _authService.GetProducts();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveCollection(CollectionRequestModel categoryRequest)
        {
            if (ModelState.IsValid)
            {
                var saveCollection = await _authService.SaveCollection(categoryRequest);
                if (saveCollection != null)
                {
                    return RedirectToAction("Collections", "Account");
                }
                else
                {
                    ModelState.AddModelError("", "Error Occured While Saving Category.");
                }
            }
            return View("CreateCollection", "Account");

        }

        [HttpGet]
        public async Task<IActionResult> HotDeals()
        {
            // Retrieve admin details from session
            var adminName = HttpContext.Session.GetString("AdminName");
            if (adminName == null)
            {
                TempData["ErrorMessage"] = "401 UnAuthorized Please login first.";
                return RedirectToAction("Login");
            }
            ViewBag.AdminName = adminName;
            var products = await _authService.GetProducts();
            if (products == null)
            {
                products = new List<ProductResponseModel> { new ProductResponseModel() };
            }
            return View(products);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateHotDealProduct(string productId, decimal discountPrice, string active, string IsProductEnabled, List<IFormFile> CategoryImages, string description, List<string> removedImages, List<string> currentImages)
        {
            try
            {
                // Find the product by ID
                var product = await _authService.UpdateHotDealProduct(productId, discountPrice, active, IsProductEnabled, CategoryImages, description, currentImages);
                if (product == null)
                {
                    return Json(new { success = false, message = "Product not found." });
                }
                return Json(new { success = true, message = "Product updated successfully!" });
            }
            catch (Exception ex)
            {
                // Log the error (not shown here for simplicity)
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Customers()
        {
            // Retrieve admin details from session
            var adminName = HttpContext.Session.GetString("AdminName");
            if (adminName == null)
            {
                TempData["ErrorMessage"] = "401 UnAuthorized Please login first.";
                return RedirectToAction("Login");
            }
            ViewBag.AdminName = adminName;
            var customers = await _authService.GetCustomers();
            if (customers == null)
            {
                customers = new List<CustomerResponseModel> { new CustomerResponseModel() };
            }
            return View(customers);
        }
        [HttpGet]
        public IActionResult LogOut()
        {
            // Clear the session
            HttpContext.Session.Clear();

            // Optionally, you can also remove specific session items
            HttpContext.Session.Remove("AdminName");

            // Redirect to the login page or another action
            return RedirectToAction("Login", "Account");
        }
        public async Task<IActionResult> ExportCustomers()
        {
            var customers = await _authService.GetCustomers(); // Fetch products from your service
            var csvData = _authService.GenerateCustomerCsv(customers);
            return File(Encoding.UTF8.GetBytes(csvData), "text/csv", "customers.csv");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCollection(int categoryId)
        {
            try
            {
                // Find the product by ID
                bool IsDeleted = await _authService.DeleteCollection(categoryId);
                if (!IsDeleted)
                {
                    return Json(new { success = false, message = "Collection not found." });
                }
                return Json(new { success = true, message = "Collection deleted successfully!" });
            }
            catch (Exception ex)
            {
                // Log the error (not shown here for simplicity)
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCollectionStatus(bool IsCollectionStatus, int categoryId)
        {
            try
            {
                // Find the product by ID
                bool IsUpdated = await _authService.UpdateCollectionStatus(IsCollectionStatus, categoryId);
                if (!IsUpdated)
                {
                    return Json(new { success = false, message = "Collection not found." });
                }
                return Json(new { success = true, message = "Collection updated successfully!" });
            }
            catch (Exception ex)
            {
                // Log the error (not shown here for simplicity)
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> DeletedCollections()
        {
            // Retrieve admin details from session
            var adminName = HttpContext.Session.GetString("AdminName");
            if (adminName == null)
            {
                TempData["ErrorMessage"] = "401 UnAuthorized Please login first.";
                return RedirectToAction("Login");
            }
            ViewBag.AdminName = adminName;
            var collections = await _authService.GetDeletedCollections();
            if (collections == null)
            {
                collections = null;
            }
            return View(collections);
        }

        public async Task<IActionResult> ResetCollection(int categoryId)
        {
            var saveCollection = await _authService.ResetCollection(categoryId);
            if (saveCollection != null)
            {
                TempData["SuccessMessage"] = "Reset successful!";
                return RedirectToAction("Collections", "Account");
            }
            else
            {
                TempData["ErrorMessage"] = "An error occurred while rest the collection";
                return RedirectToAction("DeletedCollections", "Account");
            }
        }

    }
}