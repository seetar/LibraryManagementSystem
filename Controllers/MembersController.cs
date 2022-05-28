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
using Microsoft.AspNetCore.Identity;

namespace LibraryManagementSystem.Controllers
{
    [Authorize]
    public class MembersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IPasswordHasher<IdentityUser> _passwordHash;

        public MembersController(ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            IPasswordHasher<IdentityUser> passwordHash)
        {
            _context = context;
            _userManager = userManager;
            _passwordHash = passwordHash;
        }

        // GET: Members
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index(string searchTerm)
        {
            ViewData["currentFilter"] = searchTerm;

            var members = _context.Members.ToList();

            if (!string.IsNullOrEmpty(searchTerm))
                members = members.Where(x => (x.FirstName.Trim() + " " + x.LastName.Trim()).Trim().ToLower().Contains(searchTerm.ToLower())).ToList();
            return View(members);
        }

        // GET: Members/Details/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var member = await _context.Members
                .FirstOrDefaultAsync(m => m.MemberId == id);
            if (member == null)
            {
                return NotFound();
            }

            return View(member);
        }

        // GET: Members/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            if (TempData["Errors"] != null)
                ModelState.AddModelError("", TempData["Errors"].ToString());

            return View();
        }

        // POST: Members/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("MemberId,FirstName,LastName,PhoneNumber,Email")] Member member)
        {
            if (ModelState.IsValid)
            {
                var userExists = await _userManager.FindByEmailAsync(member.Email);

                if (userExists == null)
                {
                    var user = new IdentityUser() { UserName = member.Email, Email = member.Email };

                    var result = await _userManager.CreateAsync(user, member.PhoneNumber);

                    if (result.Succeeded)
                        await _userManager.AddToRoleAsync(user, "Member");
                    else
                    {
                        AddErrors(result);
                        return RedirectToAction();
                    }
                }
                else
                {
                    TempData["Errors"] = "User already exist with the email.";
                    return RedirectToAction();
                }

                member.MemberId = Guid.NewGuid();
                _context.Add(member);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(member);
        }

        // GET: Members/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var member = await _context.Members.FindAsync(id);
            if (member == null)
            {
                return NotFound();
            }
            return View(member);
        }

        // POST: Members/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(Guid id, [Bind("MemberId,FirstName,LastName,PhoneNumber,Email")] Member member)
        {
            if (id != member.MemberId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.FindByEmailAsync(member.Email);
                    user.Email = member.Email;
                    user.PasswordHash = _passwordHash.HashPassword(user, member.PhoneNumber);
                    var result = await _userManager.UpdateAsync(user);

                    if (!result.Succeeded)
                    {
                        AddErrors(result);
                        return RedirectToAction(nameof(Index));
                    }

                    _context.Update(member);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MemberExists(member.MemberId))
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
            return View(member);
        }

        // GET: Members/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var member = await _context.Members
                .FirstOrDefaultAsync(m => m.MemberId == id);
            if (member == null)
            {
                return NotFound();
            }

            return View(member);
        }

        // POST: Members/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var member = await _context.Members.FindAsync(id);

            var issues = _context.Issues.Where(x => x.MemberId == member.MemberId).ToList();
            var books = _context.Books.ToList().Where(x => issues.Any(y => y.BookId == x.Id)).ToList();
            Parallel.ForEach(books, book => { book.NumberInStock++; });
            await _context.SaveChangesAsync();

            var user = await _userManager.FindByEmailAsync(member.Email);
            await _userManager.DeleteAsync(user);

            _context.Members.Remove(member);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Members/Account
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> Account()
        {
            var member = await GetCurrentMember();
            if (member == null)
                return NotFound();

            return View(nameof(Details), member);
        }

        // GET: Members/Issues
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> Issues(string searchTerm)
        {
            var member = await GetCurrentMember();
            if (member == null)
                return NotFound();

            ViewData["currentFilter"] = searchTerm;

            var books = _context.Issues.Include(x => x.Member).Include(y => y.Book).ThenInclude(z => z.Genre)
                .Where(w => w.MemberId == member.MemberId).Select(b => b.Book).ToList();

            if (!string.IsNullOrEmpty(searchTerm))
                books = books.Where(b => b.Name.ToLower().Contains(searchTerm.ToLower())).ToList();
            return View(books);
        }

        private bool MemberExists(Guid id)
        {
            return _context.Members.Any(e => e.MemberId == id);
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        private async Task<Member> GetCurrentMember()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);

            var member = await _context.Members
                .FirstOrDefaultAsync(x => x.Email == user.Email);
            if (member == null)
            {
                return null;
            }
            return member;
        }
    }
}
