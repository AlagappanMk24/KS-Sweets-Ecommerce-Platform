namespace KS_Sweets.Application.Contracts.Persistence
{
    public interface IUnitOfWork
    {
        ICategoryRepository Categories { get; }
        void Save();
    }
}
