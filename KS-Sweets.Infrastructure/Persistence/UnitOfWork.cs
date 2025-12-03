using KS_Sweets.Application.Contracts.Persistence;
using KS_Sweets.Infrastructure.Data.Context;
using KS_Sweets.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.Logging;

namespace KS_Sweets.Infrastructure.Persistence
{
    public class UnitOfWork(ApplicationDbContext dbContext, ILogger<UnitOfWork> logger) : IUnitOfWork
    {
        private readonly ApplicationDbContext _dbContext = dbContext;
        private readonly ILogger<UnitOfWork> _logger = logger;
        public ICategoryRepository Categories => new CategoryRepository(_dbContext);
        public void Save()
        {
            try
            {
                _dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving changes in UnitOfWork");
                throw;
            }
        }
        // Implement IDisposable for proper resource management
        public void Dispose()
        {
            _dbContext?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
