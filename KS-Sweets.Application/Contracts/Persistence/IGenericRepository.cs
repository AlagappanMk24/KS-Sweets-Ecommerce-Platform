using System.Linq.Expressions;

namespace KS_Sweets.Application.Contracts.Persistence
{
    public interface IGenericRepository<T> where T : class
    {
        void Add(T entity);
        void Attach(T entity);
        int Count(Expression<Func<T, bool>>? filter = null);
        T Get(Expression<Func<T, bool>> filter, string? includeProperties = null, bool tracked = false);
        IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter = null, string? includeProperties = null);
        T GetNoTracking(Expression<Func<T, bool>> filter, string? includeProperties = null);
        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entity);
        void Update(T entity);
        void UpdateStatus(int id, string orderStatus, string? paymentStatus = null);
        void UpdateStripePaymentID(int id, string sessionId, string paymentIntentId);
    }
}
