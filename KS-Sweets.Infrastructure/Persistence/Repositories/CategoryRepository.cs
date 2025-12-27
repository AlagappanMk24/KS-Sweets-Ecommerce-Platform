using KS_Sweets.Application.Contracts.Persistence;
using KS_Sweets.Domain.Entities;
using KS_Sweets.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace KS_Sweets.Infrastructure.Persistence.Repositories
{
    public class CategoryRepository(ApplicationDbContext dbContext) : GenericRepository<Category>(dbContext), ICategoryRepository
    {
        private readonly ApplicationDbContext _dbContext = dbContext;

        // ---------- CREATE ----------
        public bool ExistsByName(string name)
        {
            var normalized = name.Trim().ToLower();

            return _dbContext.Categories
                .Any(c => !c.IsDeleted && c.Name.ToLower() == normalized);
        }

        public bool ExistsBySlug(string slug)
        {
            var normalized = slug.Trim().ToLower();

            return _dbContext.Categories
                .Any(c => !c.IsDeleted && c.Slug.ToLower() == normalized);
        }

        // ---------- EDIT ----------
        public bool ExistsByName(string name, int excludeId)
        {
            var normalized = name.Trim().ToLower();

            return _dbContext.Categories
                .Any(c => !c.IsDeleted &&
                          c.Id != excludeId &&
                          c.Name.ToLower() == normalized);
        }
        public bool ExistsBySlug(string slug, int excludeId)
        {
            var normalized = slug.Trim().ToLower();

            return _dbContext.Categories
                .Any(c => !c.IsDeleted && c.Id != excludeId && c.Slug.ToLower() == normalized);
        }

        // ---------- Bulk Status Update ----------
        public void BulkUpdateStatus(int[] ids, bool isActive)
        {
            _dbContext.Categories
                .Where(c => ids.Contains(c.Id))
                .ExecuteUpdate(setters => setters
                    .SetProperty(c => c.IsActive, isActive)
                    .SetProperty(c => c.UpdatedAt, DateTime.UtcNow)
                );
        }
    }
}