using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartStationerySystem.Data;

namespace SmartStationerySystem.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("UserRole") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (HttpContext.Session.GetString("UserRole") != "Stationer")
            {
                return RedirectToAction("Index", "Orders");
            }

            ViewBag.TotalOrders = _context.Orders.Count();
            ViewBag.WaitingOrders = _context.Orders.Count(o => o.Status == "Beklemede");
            ViewBag.QueueOrders = _context.Orders.Count(o => o.Status == "Sırada");
            ViewBag.CompletedOrders = _context.Orders.Count(o => o.Status == "Tamamlandı");
            ViewBag.TotalRevenue = _context.Payments.Sum(p => (decimal?)p.Amount) ?? 0;
            ViewBag.TotalCustomers = _context.Customers.Count();
            ViewBag.TotalPrintOptions = _context.PrintOptions.Count();
            ViewBag.TotalPayments = _context.Payments.Count();

            ViewBag.LastOrders = _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.PrintOption)
                .OrderByDescending(o => o.CreatedDate)
                .Take(5)
                .ToList();

            return View();
        }
    }
}