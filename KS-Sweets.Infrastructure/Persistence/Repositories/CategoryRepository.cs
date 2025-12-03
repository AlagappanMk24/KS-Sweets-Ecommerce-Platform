using KS_Sweets.Application.Contracts.Persistence;
using KS_Sweets.Domain.Entities;
using KS_Sweets.Infrastructure.Data.Context;

namespace KS_Sweets.Infrastructure.Persistence.Repositories
{
    public class CategoryRepository(ApplicationDbContext dbContext) : GenericRepository<Category>(dbContext), ICategoryRepository
    {
    }
}