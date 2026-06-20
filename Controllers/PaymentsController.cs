using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartStationerySystem.Data;
using SmartStationerySystem.Models;

namespace SmartStationerySystem.Controllers
{
    public class PaymentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PaymentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var payments = _context.Payments
                .Include(p => p.Order)
                    .ThenInclude(o => o.Customer)
                .Include(p => p.Order)
                    .ThenInclude(o => o.PrintOption)
                .OrderByDescending(p => p.PaymentDate);

            return View(await payments.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var payment = await _context.Payments
                .Include(p => p.Order)
                    .ThenInclude(o => o.Customer)
                .Include(p => p.Order)
                    .ThenInclude(o => o.PrintOption)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (payment == null)
                return NotFound();

            return View(payment);
        }

        public IActionResult Create()
        {
            LoadDropdowns();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OrderId,PaymentMethod")] Payment payment)
        {
            var order = await _context.Orders.FindAsync(payment.OrderId);

            if (order == null)
                ModelState.AddModelError("", "Sipariş bulunamadı.");

            if (string.IsNullOrWhiteSpace(payment.PaymentMethod))
                ModelState.AddModelError("PaymentMethod", "Ödeme yöntemi seçilmelidir.");

            if (ModelState.IsValid && order != null)
            {
                payment.Amount = order.TotalPrice;
                payment.PaymentStatus = "Ödendi";
                payment.PaymentDate = DateTime.Now;

                order.Status = "Tamamlandı";

                var queueItem = await _context.QueueItems
                    .FirstOrDefaultAsync(q => q.OrderId == order.Id);

                if (queueItem != null)
                {
                    queueItem.IsCompleted = true;
                }

                _context.Payments.Add(payment);
                _context.Orders.Update(order);

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            LoadDropdowns(payment.OrderId);
            return View(payment);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var payment = await _context.Payments.FindAsync(id);

            if (payment == null)
                return NotFound();

            LoadDropdowns(payment.OrderId);
            return View(payment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,OrderId,PaymentMethod,PaymentStatus")] Payment payment)
        {
            if (id != payment.Id)
                return NotFound();

            var existingPayment = await _context.Payments.FindAsync(id);
            var order = await _context.Orders.FindAsync(payment.OrderId);

            if (existingPayment == null || order == null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(payment.PaymentMethod))
                ModelState.AddModelError("PaymentMethod", "Ödeme yöntemi seçilmelidir.");

            if (ModelState.IsValid)
            {
                existingPayment.OrderId = payment.OrderId;
                existingPayment.Amount = order.TotalPrice;
                existingPayment.PaymentMethod = payment.PaymentMethod;
                existingPayment.PaymentStatus = payment.PaymentStatus;
                existingPayment.PaymentDate = DateTime.Now;

                if (payment.PaymentStatus == "Ödendi")
                {
                    order.Status = "Tamamlandı";

                    var queueItem = await _context.QueueItems
                        .FirstOrDefaultAsync(q => q.OrderId == order.Id);

                    if (queueItem != null)
                    {
                        queueItem.IsCompleted = true;
                    }
                }
                else if (payment.PaymentStatus == "Beklemede")
                {
                    order.Status = "Beklemede";

                    var queueItem = await _context.QueueItems
                        .FirstOrDefaultAsync(q => q.OrderId == order.Id);

                    if (queueItem != null)
                    {
                        queueItem.IsCompleted = false;
                    }
                }
                else if (payment.PaymentStatus == "İptal")
                {
                    order.Status = "İptal Edildi";

                    var queueItem = await _context.QueueItems
                        .FirstOrDefaultAsync(q => q.OrderId == order.Id);

                    if (queueItem != null)
                    {
                        queueItem.IsCompleted = false;
                    }
                }

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            LoadDropdowns(payment.OrderId);
            return View(payment);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var payment = await _context.Payments
                .Include(p => p.Order)
                    .ThenInclude(o => o.Customer)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (payment == null)
                return NotFound();

            return View(payment);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var payment = await _context.Payments
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (payment != null)
            {
                if (payment.Order != null)
                {
                    payment.Order.Status = "Beklemede";

                    var queueItem = await _context.QueueItems
                        .FirstOrDefaultAsync(q => q.OrderId == payment.OrderId);

                    if (queueItem != null)
                    {
                        queueItem.IsCompleted = false;
                    }
                }

                _context.Payments.Remove(payment);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private void LoadDropdowns(int? selectedOrderId = null)
        {
            ViewData["OrderId"] = new SelectList(
                _context.Orders
                    .Include(o => o.Customer)
                    .Include(o => o.PrintOption)
                    .OrderByDescending(o => o.CreatedDate)
                    .Select(o => new
                    {
                        o.Id,
                        DisplayName = "Sipariş #" + o.Id + " - " +
                                      o.Customer!.FullName + " - " +
                                      o.PrintOption!.ColorType + " " +
                                      o.PrintOption.PaperSize + " - " +
                                      o.TotalPrice + " TL"
                    }),
                "Id",
                "DisplayName",
                selectedOrderId
            );

            ViewData["PaymentMethods"] = new SelectList(new[]
            {
                "Nakit",
                "Kart",
                "Online"
            });

            ViewData["PaymentStatuses"] = new SelectList(new[]
            {
                "Beklemede",
                "Ödendi",
                "İptal"
            });
        }

        private bool PaymentExists(int id)
        {
            return _context.Payments.Any(e => e.Id == id);
        }
    }
}