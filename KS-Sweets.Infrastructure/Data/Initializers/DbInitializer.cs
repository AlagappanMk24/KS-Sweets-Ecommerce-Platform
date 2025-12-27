using KS_Sweets.Domain.Constants;
using KS_Sweets.Domain.Entities;
using KS_Sweets.Domain.Entities.Identity;
using KS_Sweets.Infrastructure.Data.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace KS_Sweets.Infrastructure.Data.Initializers
{
    public class DbInitializer(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ApplicationDbContext dbContext,
        IConfiguration configuration) : IDbInitializer
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;
        private readonly ApplicationDbContext _dbContext = dbContext;
        private readonly IConfiguration _configuration = configuration;

        public async Task InitializeAsync()
        {
            await ApplyMigrationsAsync();

            await SeedRolesAsync();

            await CreateAdminUserAsync();

            // Seed categories + 6 products + images
            await SeedDataAsync();
        }
        private async Task ApplyMigrationsAsync()
        {
            if ((await _dbContext.Database.GetPendingMigrationsAsync()).Any())
            {
                await _dbContext.Database.MigrateAsync();
            }
        }
        private async Task SeedRolesAsync()
        {
            var roles = new[] { AppRoles.Admin, AppRoles.Employee, AppRoles.Customer };

            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }
        private async Task CreateAdminUserAsync()
        {
            var email = _configuration["AdminUser:Email"];
            var password = _configuration["AdminUser:Password"];
            var name = _configuration["AdminUser:Name"];

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return;

            var adminUser = await _userManager.FindByEmailAsync(email);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    Name = name,
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };

                var createResult = await _userManager.CreateAsync(adminUser, password);

                if (!createResult.Succeeded)
                    throw new Exception(string.Join(",", createResult.Errors.Select(e => e.Description)));
            }

            // ✅ ALWAYS CHECK role assignment separately
            if (!await _userManager.IsInRoleAsync(adminUser, AppRoles.Admin))
            {
                var roleResult = await _userManager.AddToRoleAsync(adminUser, AppRoles.Admin);

                if (!roleResult.Succeeded)
                    throw new Exception(string.Join(",", roleResult.Errors.Select(e => e.Description)));
            }
        }
        private async Task SeedDataAsync()
        {
            if (await _dbContext.Categories.AnyAsync()) return;

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

            await _dbContext.Categories.AddRangeAsync(categories);
            await _dbContext.SaveChangesAsync();
        }
    }
}