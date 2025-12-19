using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

using ErasmusAtlas.Extensions;
using ErasmusAtlas.Infrastructure;
using ErasmusAtlas.Infrastructure.Models;

namespace ErasmusAtlas
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<ErasmusAtlasDbContext>(options =>
            {
                options.UseNpgsql(
                    builder.Configuration.GetConnectionString("DefaultConnection"),
                    npgsqlOptions =>
                    {
                        npgsqlOptions.UseNetTopologySuite();
                    });
            });

            builder.Services.AddIdentity<ErasmusUser, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 6;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<ErasmusAtlasDbContext>()
            .AddDefaultTokenProviders();

            builder.Services.AddControllersWithViews();

            builder.Services.RegisterRepositories();
            builder.Services.RegisterUserDefinedServices();

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
