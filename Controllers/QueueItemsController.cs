using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartStationerySystem.Data;
using SmartStationerySystem.Models;

namespace SmartStationerySystem.Controllers
{
    public class QueueItemsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public QueueItemsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var queueItems = _context.QueueItems
                .Include(q => q.Order)
                    .ThenInclude(o => o.Customer)
                .Include(q => q.Order)
                    .ThenInclude(o => o.PrintOption)
                .OrderBy(q => q.QueueNumber);

            return View(await queueItems.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var queueItem = await _context.QueueItems
                .Include(q => q.Order)
                    .ThenInclude(o => o.Customer)
                .Include(q => q.Order)
                    .ThenInclude(o => o.PrintOption)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (queueItem == null)
                return NotFound();

            return View(queueItem);
        }

        public IActionResult Create()
        {
            LoadOrders();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OrderId")] QueueItem queueItem)
        {
            var order = await _context.Orders.FindAsync(queueItem.OrderId);

            if (order == null)
                ModelState.AddModelError("", "Sipariş bulunamadı.");

            var alreadyExists = await _context.QueueItems.AnyAsync(q => q.OrderId == queueItem.OrderId);

            if (alreadyExists)
                ModelState.AddModelError("", "Bu sipariş zaten kuyrukta.");

            if (ModelState.IsValid && order != null)
            {
                int nextQueueNumber = 1;

                if (await _context.QueueItems.AnyAsync())
                    nextQueueNumber = await _context.QueueItems.MaxAsync(q => q.QueueNumber) + 1;

                queueItem.QueueNumber = nextQueueNumber;
                queueItem.AddedTime = DateTime.Now;
                queueItem.IsCompleted = false;

                order.Status = "Sırada";

                _context.QueueItems.Add(queueItem);
                _context.Orders.Update(order);

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            LoadOrders(queueItem.OrderId);
            return View(queueItem);
        }

        public async Task<IActionResult> MarkCompleted(int id)
        {
            var queueItem = await _context.QueueItems
                .Include(q => q.Order)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (queueItem == null)
                return NotFound();

            queueItem.IsCompleted = true;

            if (queueItem.Order != null)
                queueItem.Order.Status = "Tamamlandı";

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var queueItem = await _context.QueueItems
                .Include(q => q.Order)
                    .ThenInclude(o => o.Customer)
                .Include(q => q.Order)
                    .ThenInclude(o => o.PrintOption)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (queueItem == null)
                return NotFound();

            return View(queueItem);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var queueItem = await _context.QueueItems.FindAsync(id);

            if (queueItem != null)
                _context.QueueItems.Remove(queueItem);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private void LoadOrders(int? selectedOrderId = null)
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
                                      o.PrintOption.PaperSize
                    }),
                "Id",
                "DisplayName",
                selectedOrderId
            );
        }

        private bool QueueItemExists(int id)
        {
            return _context.QueueItems.Any(e => e.Id == id);
        }
    }
}