namespace KS_Sweets.Infrastructure.Data.Initializers
{
    public interface IDbInitializer
    {
        void Initialize();
        void SeedData();
    }
}
