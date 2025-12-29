using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HelpDeskKyotera.Data;
using HelpDeskKyotera.Models;

namespace HelpDeskKyotera
{
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Categories
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Categories.Include(c => c.DefaultTeam).Include(c => c.Parent);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Categories/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .Include(c => c.DefaultTeam)
                .Include(c => c.Parent)
                .FirstOrDefaultAsync(m => m.CategoryId == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // GET: Categories/Create
        public IActionResult Create()
        {
            ViewData["DefaultTeamId"] = new SelectList(_context.Teams.OrderBy(t => t.Name), "TeamId", "Name");
            ViewData["ParentId"] = new SelectList(_context.Categories.OrderBy(c => c.Name), "CategoryId", "Name");
            return View();
        }

        // POST: Categories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CategoryId,Name,Description,ParentId,DefaultTeamId")] Category category)
        {
            if (ModelState.IsValid)
            {
                category.CategoryId = Guid.NewGuid();
                _context.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["DefaultTeamId"] = new SelectList(_context.Teams.OrderBy(t => t.Name), "TeamId", "Name", category.DefaultTeamId);
            ViewData["ParentId"] = new SelectList(_context.Categories.OrderBy(c => c.Name), "CategoryId", "Name", category.ParentId);
            return View(category);
        }

        // GET: Categories/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            ViewData["DefaultTeamId"] = new SelectList(_context.Teams.OrderBy(t => t.Name), "TeamId", "Name", category.DefaultTeamId);
            ViewData["ParentId"] = new SelectList(_context.Categories.Where(c => c.CategoryId != id).OrderBy(c => c.Name), "CategoryId", "Name", category.ParentId);
            return View(category);
        }

        // POST: Categories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("CategoryId,Name,Description,ParentId,DefaultTeamId")] Category category)
        {
            if (id != category.CategoryId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.CategoryId))
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
            ViewData["DefaultTeamId"] = new SelectList(_context.Teams.OrderBy(t => t.Name), "TeamId", "Name", category.DefaultTeamId);
            ViewData["ParentId"] = new SelectList(_context.Categories.Where(c => c.CategoryId != category.CategoryId).OrderBy(c => c.Name), "CategoryId", "Name", category.ParentId);
            return View(category);
        }

        // GET: Categories/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .Include(c => c.DefaultTeam)
                .Include(c => c.Parent)
                .FirstOrDefaultAsync(m => m.CategoryId == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var category = await _context.Categories
                .Include(c => c.Children)
                .Include(c => c.Tickets)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
            {
                return RedirectToAction(nameof(Index));
            }

            if ((category.Children != null && category.Children.Any()) || (category.Tickets != null && category.Tickets.Any()))
            {
                TempData["Error"] = "Cannot delete category that has child categories or tickets assigned.";
                return RedirectToAction(nameof(Delete), new { id });
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(Guid id)
        {
            return _context.Categories.Any(e => e.CategoryId == id);
        }
    }
}
