using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartStationerySystem.Data;
using SmartStationerySystem.Models;

namespace SmartStationerySystem.Controllers
{
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public OrdersController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index(string? searchString, string? statusFilter)
        {
            if (HttpContext.Session.GetString("UserRole") == null)
                return RedirectToAction("Login", "Account");

            var orders = _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.PrintOption)
                .Include(o => o.UploadedFile)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                orders = orders.Where(o =>
                    o.Customer != null &&
                    o.Customer.FullName.Contains(searchString));
            }

            if (!string.IsNullOrWhiteSpace(statusFilter))
            {
                orders = orders.Where(o => o.Status == statusFilter);
            }

            ViewBag.SearchString = searchString;
            ViewBag.StatusFilter = statusFilter;

            return View(await orders
                .OrderByDescending(o => o.CreatedDate)
                .ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (HttpContext.Session.GetString("UserRole") == null)
                return RedirectToAction("Login", "Account");

            if (id == null)
                return NotFound();

            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.PrintOption)
                .Include(o => o.UploadedFile)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (order == null)
                return NotFound();

            return View(order);
        }

        public IActionResult Create()
        {
            if (HttpContext.Session.GetString("UserRole") == null)
                return RedirectToAction("Login", "Account");

            LoadDropdowns();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("CustomerId,PrintOptionId,PageCount,CopyCount")] Order order,
            IFormFile? uploadedDocument)
        {
            if (HttpContext.Session.GetString("UserRole") == null)
                return RedirectToAction("Login", "Account");

            var printOption = await _context.PrintOptions.FindAsync(order.PrintOptionId);

            if (printOption == null)
                ModelState.AddModelError("", "Baskı seçeneği bulunamadı.");

            if (order.PageCount <= 0)
                ModelState.AddModelError("PageCount", "Sayfa sayısı 0'dan büyük olmalıdır.");

            if (order.CopyCount <= 0)
                ModelState.AddModelError("CopyCount", "Kopya sayısı 0'dan büyük olmalıdır.");

            if (uploadedDocument != null && uploadedDocument.Length > 0)
            {
                var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".jpg", ".jpeg", ".png" };
                var extension = Path.GetExtension(uploadedDocument.FileName).ToLower();

                if (!allowedExtensions.Contains(extension))
                    ModelState.AddModelError("", "Sadece PDF, Word veya görsel dosyaları yüklenebilir.");

                if (uploadedDocument.Length > 10 * 1024 * 1024)
                    ModelState.AddModelError("", "Dosya boyutu 10 MB'dan küçük olmalıdır.");
            }

            if (ModelState.IsValid && printOption != null)
            {
                order.TotalPrice = order.PageCount * order.CopyCount * printOption.PricePerPage;
                order.Status = "Beklemede";
                order.CreatedDate = DateTime.Now;

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                if (uploadedDocument != null && uploadedDocument.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");

                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    string uniqueFileName = Guid.NewGuid() + "_" + Path.GetFileName(uploadedDocument.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await uploadedDocument.CopyToAsync(fileStream);
                    }

                    var uploadedFile = new UploadedFile
                    {
                        OrderId = order.Id,
                        FileName = uploadedDocument.FileName,
                        FilePath = "/uploads/" + uniqueFileName,
                        UploadDate = DateTime.Now
                    };

                    _context.UploadedFiles.Add(uploadedFile);
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }

            LoadDropdowns(order.CustomerId, order.PrintOptionId);
            return View(order);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (HttpContext.Session.GetString("UserRole") == null)
                return RedirectToAction("Login", "Account");

            if (id == null)
                return NotFound();

            var order = await _context.Orders.FindAsync(id);

            if (order == null)
                return NotFound();

            LoadDropdowns(order.CustomerId, order.PrintOptionId);
            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CustomerId,PrintOptionId,PageCount,CopyCount,Status,CreatedDate")] Order order)
        {
            if (HttpContext.Session.GetString("UserRole") == null)
                return RedirectToAction("Login", "Account");

            if (id != order.Id)
                return NotFound();

            var printOption = await _context.PrintOptions.FindAsync(order.PrintOptionId);

            if (printOption == null)
                ModelState.AddModelError("", "Baskı seçeneği bulunamadı.");

            if (order.PageCount <= 0)
                ModelState.AddModelError("PageCount", "Sayfa sayısı 0'dan büyük olmalıdır.");

            if (order.CopyCount <= 0)
                ModelState.AddModelError("CopyCount", "Kopya sayısı 0'dan büyük olmalıdır.");

            if (ModelState.IsValid && printOption != null)
            {
                order.TotalPrice = order.PageCount * order.CopyCount * printOption.PricePerPage;
                _context.Update(order);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            LoadDropdowns(order.CustomerId, order.PrintOptionId);
            return View(order);
        }

        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            if (HttpContext.Session.GetString("UserRole") == null)
                return RedirectToAction("Login", "Account");

            var allowedStatuses = new[] { "Beklemede", "Sırada", "Hazırlanıyor", "Tamamlandı", "İptal Edildi" };

            if (!allowedStatuses.Contains(status))
                return BadRequest("Geçersiz sipariş durumu.");

            var order = await _context.Orders.FindAsync(id);

            if (order == null)
                return NotFound();

            order.Status = status;

            if (status == "Sırada")
            {
                var alreadyInQueue = await _context.QueueItems.AnyAsync(q => q.OrderId == id);

                if (!alreadyInQueue)
                {
                    int nextQueueNumber = await _context.QueueItems.AnyAsync()
                        ? await _context.QueueItems.MaxAsync(q => q.QueueNumber) + 1
                        : 1;

                    _context.QueueItems.Add(new QueueItem
                    {
                        OrderId = id,
                        QueueNumber = nextQueueNumber,
                        AddedTime = DateTime.Now,
                        IsCompleted = false
                    });
                }
            }

            if (status == "Tamamlandı")
            {
                var queueItem = await _context.QueueItems.FirstOrDefaultAsync(q => q.OrderId == id);

                if (queueItem != null)
                    queueItem.IsCompleted = true;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (HttpContext.Session.GetString("UserRole") == null)
                return RedirectToAction("Login", "Account");

            if (id == null)
                return NotFound();

            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.PrintOption)
                .Include(o => o.UploadedFile)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (order == null)
                return NotFound();

            return View(order);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (HttpContext.Session.GetString("UserRole") == null)
                return RedirectToAction("Login", "Account");

            var order = await _context.Orders
                .Include(o => o.UploadedFile)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order != null)
            {
                if (order.UploadedFile != null)
                {
                    string fileFullPath = Path.Combine(
                        _webHostEnvironment.WebRootPath,
                        order.UploadedFile.FilePath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString())
                    );

                    if (System.IO.File.Exists(fileFullPath))
                        System.IO.File.Delete(fileFullPath);
                }

                _context.Orders.Remove(order);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private void LoadDropdowns(int? selectedCustomerId = null, int? selectedPrintOptionId = null)
        {
            ViewData["CustomerId"] = new SelectList(
                _context.Customers.OrderBy(c => c.FullName),
                "Id",
                "FullName",
                selectedCustomerId
            );

            ViewData["PrintOptionId"] = new SelectList(
                _context.PrintOptions
                    .OrderBy(p => p.ColorType)
                    .ThenBy(p => p.PaperSize)
                    .Select(p => new
                    {
                        p.Id,
                        DisplayName = p.ColorType + " - " + p.PaperSize + " - " +
                                      (p.IsDoubleSided ? "Arkalı Önlü" : "Tek Yüz") +
                                      " - " + p.PricePerPage + " TL"
                    }),
                "Id",
                "DisplayName",
                selectedPrintOptionId
            );
        }

        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.Id == id);
        }
    }
}