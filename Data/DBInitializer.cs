using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryManagementSystem.Data
{
    public class DBInitializer
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _config;

        public DBInitializer(ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration config)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _config = config;
        }

        public void SeedData()
        {
            Seed().Wait();
        }

        private async Task Seed()
        {
            string[] roles = { "Admin", "Member" };

            foreach (var role in roles)
            {
                var roleExists = await _roleManager.RoleExistsAsync(role);

                if (!roleExists)
                    await _roleManager.CreateAsync(new IdentityRole() { Name = role });
            }

            string email = _config["Email"];
            string password = _config["Password"];

            if (!string.IsNullOrWhiteSpace(email) && !string.IsNullOrWhiteSpace(password))
            {
                var userExists = await _userManager.FindByEmailAsync(email);

                if (userExists == null)
                {
                    var user = new IdentityUser() { UserName = email, Email = email };

                    var result = await _userManager.CreateAsync(user, password);

                    if (result.Succeeded)
                        await _userManager.AddToRoleAsync(user, "Admin");
                }
            }
            _context.SaveChanges();
        }
    }
}
