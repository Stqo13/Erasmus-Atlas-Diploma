using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

using ErasmusAtlas.Extensions;
using ErasmusAtlas.Infrastructure;
using ErasmusAtlas.Infrastructure.Models;
using ErasmusAtlas.Infrastructure.RunTimeSeeders;

namespace ErasmusAtlas
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<ErasmusAtlasDbContext>(options =>
            {
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection"),
                    x => x.UseNetTopologySuite());
            });

            builder.Services.AddIdentity<ErasmusUser, IdentityRole>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireDigit = false;
            })
            .AddEntityFrameworkStores<ErasmusAtlasDbContext>()
            .AddDefaultTokenProviders();

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.LogoutPath = "/Account/Logout";
                options.AccessDeniedPath = "/Account/AccessDenied";
                options.SlidingExpiration = true;
            });

            builder.Services.AddControllersWithViews();

            builder.Services.RegisterRepositories();
            builder.Services.RegisterUserDefinedServices();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ErasmusAtlasDbContext>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ErasmusUser>>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                await db.Database.MigrateAsync();

                await IdentitySeeder.SeedRolesAsync(roleManager);
                var users = await IdentitySeeder.SeedUsersAsync(userManager);
                var userIds = users.Select(u => u.Id).ToList();

                await PostSeeder.SeedPostsAsync(db, userIds);

                await InstitutionSeeder.SeedAsync(db);
                await ProjectSeeder.SeedAsync(db, totalProjects: 120);
                await ProjectApplicationSeeder.SeedAsync(db, userIds, maxApplicationsPerProject: 3);
            }

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
