using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;

namespace LibraryManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class IssuesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public IssuesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Issues
        public async Task<IActionResult> Index(string searchTerm)
        {
            if (TempData["Errors"] != null)
                ModelState.AddModelError("", TempData["Errors"].ToString());

            ViewData["currentFilter"] = searchTerm;

            var issues = _context.Issues.Include(x => x.Member).Include(y => y.Book).ToList();

            if (!string.IsNullOrEmpty(searchTerm))
                issues = issues.Where(x => x.Book.Name.ToLower().Contains(searchTerm.ToLower())
                || (x.Member.FirstName.Trim() + " " + x.Member.LastName.Trim()).Trim().ToLower().Contains(searchTerm.ToLower())).ToList();

            ViewData["BookId"] = new SelectList(_context.Books.Where(x => x.NumberInStock > 0), "Id", "Name");
            ViewData["MemberId"] = new SelectList(_context.Members, "MemberId", "FirstName");

            return View(issues);
        }

        // POST: Issues/Issue
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Issue([Bind("IssueId,MemberId,BookId")] Issue issue)
        {
            if (ModelState.IsValid)
            {
                if (_context.Issues.Any(x => x.BookId == issue.BookId && x.MemberId == issue.MemberId))
                {
                    var member = _context.Members.Find(issue.MemberId);
                    TempData["Errors"] = $"{member.FirstName} {member.LastName} already has this book.";
                    return RedirectToAction(nameof(Index));
                }

                issue.IssueId = Guid.NewGuid();
                _context.Add(issue);
                await _context.SaveChangesAsync();

                var book = await _context.Books.FindAsync(issue.BookId);
                book.NumberInStock--;
                _context.Update(book);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["BookId"] = new SelectList(_context.Books.Where(x => x.NumberInStock > 0), "Id", "Name", issue.BookId);
            ViewData["MemberId"] = new SelectList(_context.Members, "MemberId", "FirstName", issue.MemberId);
            return View(issue);
        }

        // POST: Issues/Return/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Return(Guid id)
        {
            var issue = await _context.Issues.FindAsync(id);
            _context.Issues.Remove(issue);
            await _context.SaveChangesAsync();

            var book = await _context.Books.FindAsync(issue.BookId);
            book.NumberInStock++;
            _context.Update(book);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
