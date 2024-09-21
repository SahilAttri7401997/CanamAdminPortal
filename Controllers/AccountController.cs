using CanamDistributors.Interfaces;
using CanamDistributors.Models;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace CanamDistributors.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IQuickBookService _quickBookService;
        private readonly string _uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
        public AccountController(IAuthService authService, IQuickBookService quickBookService)
        {
            _authService = authService;
            _quickBookService = quickBookService;
            if (!Directory.Exists(_uploadFolder))
            {
                Directory.CreateDirectory(_uploadFolder);
            }
        }

        [HttpGet]
        public IActionResult Login()
        {
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
                    TempData["SuccessMessage"] = "Login successful!";
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
            ViewBag.AdminName = adminName;
            var products = await _authService.GetProducts();
            if (products == null)
            {
                products = new List<Entity.Products> { new Entity.Products() };
            }
            return View(products);
        }

        [HttpGet]
        public async Task<IActionResult> CreateProduct()
        {
            // Retrieve admin details from session
            var adminName = HttpContext.Session.GetString("AdminName");
            ViewBag.AdminName = adminName;
            ViewBag.CategoryList = await _authService.GetCollections();
            return View();
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
            ViewBag.AdminName = adminName;
            ViewBag.ProductList = await _authService.GetProducts();
            return View();
        }

        [HttpPost]
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

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            try
            {
                var filePath = Path.Combine(_uploadFolder, file.FileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return Ok(new { filePath });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> HotDeals()
        {
            // Retrieve admin details from session
            var adminName = HttpContext.Session.GetString("AdminName");
            ViewBag.AdminName = adminName;
            var products = await _authService.GetProducts();
            foreach (var item in products)
            {
                item.Active = "false";
            }
            if (products == null)
            {
                products = new List<Entity.Products> { new Entity.Products() };
            }
            return View(products);
        }

        [HttpGet]
        public IActionResult CreditAccountForm()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveCustomer(CreditAccountFormRequestModel model)
        {
            if (ModelState.IsValid)
            {
                var customerModel = await _authService.SaveCustomer(model);
                if (customerModel != null)
                {
                    TempData["SuccessMessage"] = "Register successful!";
                    return RedirectToAction("CreditAccountForm"); // Redirect to avoid resubmission
                }
            }
            TempData["ErrorMessage"] = "There was an error processing your request.";
            return View("CreditAccountForm", model);
        }

    }
}
