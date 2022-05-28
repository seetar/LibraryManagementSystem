using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryManagementSystem.Data
{
    public static class DatabaseManager
    {
        public static IHost SeedData(this IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var context = services.GetService<ApplicationDbContext>();
                var userManager = services.GetService<UserManager<IdentityUser>>();
                var roleManager = services.GetService<RoleManager<IdentityRole>>();
                var config = services.GetService<IConfiguration>();

                context.Database.Migrate();

                new DBInitializer(context, userManager, roleManager, config).SeedData();
            }
            return host;
        }
    }
}
