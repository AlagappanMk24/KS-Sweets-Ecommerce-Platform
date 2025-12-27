using KS_Sweets.Domain.Entities;

namespace KS_Sweets.Application.Contracts.Persistence
{
    public interface ICategoryRepository : IGenericRepository<Category>
    {
        bool ExistsByName(string name);
        bool ExistsBySlug(string slug);
        bool ExistsByName(string name, int excludeId);
        bool ExistsBySlug(string slug, int excludeId);
        void BulkUpdateStatus(int[] ids, bool isActive);
    }
}