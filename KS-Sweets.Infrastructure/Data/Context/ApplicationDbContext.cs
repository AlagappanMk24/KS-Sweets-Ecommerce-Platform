using KS_Sweets.Domain.Entities;
using KS_Sweets.Domain.Entities.Common;
using KS_Sweets.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Reflection;

namespace KS_Sweets.Infrastructure.Data.Context
{
    /// <summary>
    /// Application database context containing all entity configurations,
    /// identity configuration, audit filters, unique constraints, relationships,
    /// and navigation mappings.
    /// </summary>
    /// <remarks>
    /// Constructor that initializes the database context with the given options.
    /// </remarks>
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        #region DbSets

        /// <summary>Category master table.</summary>
        public DbSet<Category> Categories { get; set; }

        /// <summary>Product master table.</summary>
        public DbSet<Product> Products { get; set; }

        /// <summary>Application users (extends IdentityUser).</summary>
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }

        /// <summary>Product image gallery.</summary>
        public DbSet<ProductImage> ProductImages { get; set; }

        /// <summary>User shopping cart items.</summary>
        public DbSet<ShoppingCart> ShoppingCarts { get; set; }

        /// <summary>User wishlist items.</summary>
        public DbSet<WishList> WishLists { get; set; }

        /// <summary>Order header (master) table.</summary>
        public DbSet<OrderHeader> OrderHeaders { get; set; }

        /// <summary>Order details (child items) table.</summary>
        public DbSet<OrderDetails> OrderDetails { get; set; }

        /// <summary>User feedback and reviews.</summary>
        public DbSet<Feedback> Feedbacks { get; set; }

        /// <summary>User notifications.</summary>
        public DbSet<Notification> Notifications { get; set; }

        #endregion


        /// <summary>
        /// Configures entity mappings, relationships, constraints, and global filters.
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            // -------------------------------------------------------------
            // 1️⃣ Unique Index Configuration (Industry Standard)
            // -------------------------------------------------------------

            modelBuilder.Entity<Category>()
                .HasIndex(c => c.Name)
                .IsUnique();

            modelBuilder.Entity<Category>()
                .HasIndex(c => c.Slug)
                .IsUnique();

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.Name)
                .IsUnique();

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.Slug)
                .IsUnique();


            // -------------------------------------------------------------
            // 2️⃣ Soft Delete Global Filter for All BaseEntity Entities
            // -------------------------------------------------------------

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(BaseAuditEntity).IsAssignableFrom(entityType.ClrType))
                {
                    // Call the generic helper method below
                    SetGlobalQueryFilter(modelBuilder, entityType.ClrType);
                }
            }

            // -------------------------------------------------------------
            // 3️⃣ Relationship Configuration (EF Core Best-Practice)
            // -------------------------------------------------------------

            // Product → Category (many-to-one)
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict); // industry standard

            // OrderHeader → User (many-to-one)
            modelBuilder.Entity<OrderHeader>()
                .HasOne(o => o.ApplicationUser)
                .WithMany()
                .HasForeignKey(o => o.ApplicationUserId)
                .OnDelete(DeleteBehavior.NoAction);

            // OrderDetails → OrderHeader (many-to-one)
            modelBuilder.Entity<OrderDetails>()
                .HasOne(od => od.OrderHeader)
                .WithMany()
                .HasForeignKey(od => od.OrderHeaderId)
                .OnDelete(DeleteBehavior.Cascade);

            // OrderDetails → Product (many-to-one)
            modelBuilder.Entity<OrderDetails>()
                .HasOne(od => od.Product)
                .WithMany()
                .HasForeignKey(od => od.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Notification → User (many-to-one)
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ShoppingCart → User
            modelBuilder.Entity<ShoppingCart>()
                .HasOne(s => s.ApplicationUser)
                .WithMany()
                .HasForeignKey(s => s.ApplicationUserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ShoppingCart → Product
            modelBuilder.Entity<ShoppingCart>()
                .HasOne(s => s.Product)
                .WithMany()
                .HasForeignKey(s => s.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // WishList → User
            modelBuilder.Entity<WishList>()
                .HasOne(w => w.ApplicationUser)
                .WithMany()
                .HasForeignKey(w => w.ApplicationUserId)
                .OnDelete(DeleteBehavior.Cascade);

            // WishList → Product
            modelBuilder.Entity<WishList>()
                .HasOne(w => w.Product)
                .WithMany()
                .HasForeignKey(w => w.ProductId)
                .OnDelete(DeleteBehavior.Restrict);


            // -------------------------------------------------------------
            // 4️⃣ Apply Entity Configurations from Separate Files (Optional)
            //    Allows using IEntityTypeConfiguration<TEntity> classes.
            // -------------------------------------------------------------
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        
    }
    /// <summary>
    /// Dynamically builds and applies the query filter for soft-delete.
    /// </summary>
    private static void SetGlobalQueryFilter(ModelBuilder modelBuilder, Type entityType)
        {
            // 1. Get the parameter 'e' (the entity itself)
            var parameter = Expression.Parameter(entityType, "e");

            // 2. Get the property 'IsDeleted' from the entity
            var property = Expression.Property(parameter, "IsDeleted");

            // 3. Get the constant value 'false'
            var constant = Expression.Constant(false);

            // 4. Create the binary expression 'e.IsDeleted == false'
            var equal = Expression.Equal(property, constant);

            // 5. Create the Lambda expression 'e => e.IsDeleted == false'
            var lambda = Expression.Lambda(equal, parameter);

            // 6. Invoke the generic HasQueryFilter method using reflection
            modelBuilder.Entity(entityType).HasQueryFilter(lambda);
        }
    }
}