using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartStationerySystem.Data;
using SmartStationerySystem.Models;

namespace SmartStationerySystem.Controllers
{
    public class PrintOptionsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PrintOptionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: PrintOptions
        public async Task<IActionResult> Index()
        {
            return View(await _context.PrintOptions.ToListAsync());
        }

        // GET: PrintOptions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var printOption = await _context.PrintOptions
                .FirstOrDefaultAsync(m => m.Id == id);
            if (printOption == null)
            {
                return NotFound();
            }

            return View(printOption);
        }

        // GET: PrintOptions/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: PrintOptions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ColorType,PaperSize,IsDoubleSided,PricePerPage")] PrintOption printOption)
        {
            if (ModelState.IsValid)
            {
                _context.Add(printOption);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(printOption);
        }

        // GET: PrintOptions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var printOption = await _context.PrintOptions.FindAsync(id);
            if (printOption == null)
            {
                return NotFound();
            }
            return View(printOption);
        }

        // POST: PrintOptions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ColorType,PaperSize,IsDoubleSided,PricePerPage")] PrintOption printOption)
        {
            if (id != printOption.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(printOption);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PrintOptionExists(printOption.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(printOption);
        }

        // GET: PrintOptions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var printOption = await _context.PrintOptions
                .FirstOrDefaultAsync(m => m.Id == id);
            if (printOption == null)
            {
                return NotFound();
            }

            return View(printOption);
        }

        // POST: PrintOptions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var printOption = await _context.PrintOptions.FindAsync(id);
            if (printOption != null)
            {
                _context.PrintOptions.Remove(printOption);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PrintOptionExists(int id)
        {
            return _context.PrintOptions.Any(e => e.Id == id);
        }
    }
}
