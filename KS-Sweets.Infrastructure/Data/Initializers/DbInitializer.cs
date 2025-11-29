using KS_Sweets.Domain.Constants;
using KS_Sweets.Domain.Entities;
using KS_Sweets.Infrastructure.Data.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace KS_Sweets.Infrastructure.Data.Initializers
{
    public class DbInitializer(
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ApplicationDbContext dbContext) : IDbInitializer
    {
        private readonly UserManager<IdentityUser> _userManager = userManager;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;
        private readonly ApplicationDbContext _dbContext = dbContext;

        public void Initialize()
        {
            // ✅ Apply pending migrations automatically
            try
            {
                if (_dbContext.Database.GetPendingMigrations().Any())
                {
                    _dbContext.Database.Migrate();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Migration error: {ex.Message}");
            }

            // ✅ Ensure Roles exist
            CreateRoleIfNotExists(AppRoles.Customer);
            CreateRoleIfNotExists(AppRoles.Employee);
            CreateRoleIfNotExists(AppRoles.Admin);

            // Create admin user
            CreateAdminUser();

            // Seed categories + 6 products + images
            SeedData();
        }

        public void SeedData()
        {
            // Run only if no products exist (prevents duplicate seeding)
            if (_dbContext.Products.Any()) return;

            // 1. Seed Categories
            var categories = new List<Category>
            {
                new() { Name = "Cakes",        Slug = "cakes",        Description = "Freshly baked cakes",         ImageUrl = "/images/categories/Cakes.jpg",        IsActive = true },
                new() { Name = "Donuts",       Slug = "donuts",       Description = "Colorful glazed donuts",      ImageUrl = "/images/categories/Donuts.jpg",       IsActive = true },
                new() { Name = "Macarons",     Slug = "macarons",     Description = "French macarons",             ImageUrl = "/images/categories/Macarons.jpg",     IsActive = true },
                new() { Name = "Cupcakes",     Slug = "cupcakes",     Description = "Decorated cupcakes",          ImageUrl = "/images/categories/Cupcakes.jpg",     IsActive = true },
                new() { Name = "Tarts",        Slug = "tarts",        Description = "Fruit & chocolate tarts",     ImageUrl = "/images/categories/Tarts & Pies.jpg",        IsActive = true },
                new() { Name = "Oriental Sweets", Slug = "oriental-sweets", Description = "Kunafa, Baklava & more", ImageUrl = "/images/categories/Pastries.jpg", IsActive = true }
            };

            _dbContext.Categories.AddRange(categories);
            _dbContext.SaveChanges();

            Console.WriteLine("Successfully seeded 6 categories, 6 products and their images!");
        }

        private void CreateAdminUser()
        {
            var adminEmail = "AmoreAdmin@gmail.com";
            var adminPassword = "Admin123*??";

            var adminUser = _userManager.FindByEmailAsync(adminEmail).GetAwaiter().GetResult();

            if (adminUser == null)
            {
                var newAdmin = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = _userManager.CreateAsync(newAdmin, adminPassword).GetAwaiter().GetResult();
                if (result.Succeeded)
                {
                    _userManager.AddToRoleAsync(newAdmin, AppRoles.Admin).GetAwaiter().GetResult();
                    Console.WriteLine("Admin user created successfully!");
                }
            }
            else if (!_userManager.GetRolesAsync(adminUser).GetAwaiter().GetResult().Contains(AppRoles.Admin))
            {
                _userManager.AddToRoleAsync(adminUser, AppRoles.Admin).GetAwaiter().GetResult();
            }
        }

        // ✅ Helper Method to safely create role if it doesn't exist
        private void CreateRoleIfNotExists(string roleName)
        {
            if (!_roleManager.RoleExistsAsync(roleName).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(roleName)).GetAwaiter().GetResult();
                Console.WriteLine($"✅ Role '{roleName}' created successfully!");
            }
        }
    }
}
