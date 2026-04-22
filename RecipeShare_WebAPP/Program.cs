using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RecipeShare.Core.Interfaces;
using RecipeShare.Core.Services;
using RecipeShareData;
using RecipeShareData.Entities;
using RecipeShareData.Seed;

namespace RecipeShare_WebAPP
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var connectionString = builder.Configuration.GetConnectionString("Home")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            builder.Services.AddDbContext<RecipeShareContext>(options =>
                options.UseSqlServer(connectionString));

            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddDefaultIdentity<User>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<RecipeShareContext>();

			builder.Services.ConfigureApplicationCookie(options =>
			{
				options.LoginPath = "/Users/Login";
				options.AccessDeniedPath = "/Users/Login";
			});

			builder.Services.AddControllersWithViews();
			builder.Services.AddScoped<IRecipeService, RecipeService>();

			var app = builder.Build();


			using (var scope = app.Services.CreateScope())
			{
                var context = scope.ServiceProvider.GetRequiredService<RecipeShareContext>();
                CategorySeeder.SeedAsync(context).GetAwaiter().GetResult();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
                if (!roleManager.RoleExistsAsync("Admin").GetAwaiter().GetResult())
                {
                    roleManager.CreateAsync(new IdentityRole("Admin")).GetAwaiter().GetResult();
                }
                var adminEmail = "Admin@Admin.Admin"; 
                var adminUser = userManager.FindByEmailAsync(adminEmail).GetAwaiter().GetResult();

                if (adminUser != null)
                {
                    var isInRole = userManager.IsInRoleAsync(adminUser, "Admin").GetAwaiter().GetResult();
                    if (!isInRole)
                    {
                        userManager.AddToRoleAsync(adminUser, "Admin").GetAwaiter().GetResult();
                    }
                }

            }

			if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapStaticAssets();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}
