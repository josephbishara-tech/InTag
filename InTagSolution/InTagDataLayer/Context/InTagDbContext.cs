using InTagEntitiesLayer.Common;
using InTagEntitiesLayer.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace InTagDataLayer.Context
{
    public class InTagDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
    {
        private readonly ITenantService _tenantService;

        public InTagDbContext(
            DbContextOptions<InTagDbContext> options,
            ITenantService tenantService)
            : base(options)
        {
            _tenantService = tenantService;
        }

        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply all entity configurations from this assembly
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(InTagDbContext).Assembly);

            // Apply global query filters for multi-tenancy and soft delete
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
                {
                    var method = typeof(InTagDbContext)
                        .GetMethod(nameof(ApplyGlobalFilters),
                            System.Reflection.BindingFlags.NonPublic |
                            System.Reflection.BindingFlags.Static)!
                        .MakeGenericMethod(entityType.ClrType);

                    method.Invoke(null, new object[] { modelBuilder, _tenantService });
                }
            }
        }

        private static void ApplyGlobalFilters<TEntity>(
            ModelBuilder modelBuilder,
            ITenantService tenantService)
            where TEntity : BaseEntity
        {
            modelBuilder.Entity<TEntity>().HasQueryFilter(
                e => e.TenantId == tenantService.GetCurrentTenantId()
                     && e.IsActive);
        }

        public override int SaveChanges()
        {
            ApplyAuditFields();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ApplyAuditFields();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void ApplyAuditFields()
        {
            var entries = ChangeTracker.Entries<BaseEntity>();
            var now = DateTimeOffset.UtcNow;
            var userId = _tenantService.GetCurrentUserId();
            var tenantId = _tenantService.GetCurrentTenantId();

            foreach (var entry in entries)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedDate = now;
                        entry.Entity.CreatedByUserId = userId;
                        entry.Entity.TenantId = tenantId;
                        entry.Entity.IsActive = true;
                        break;

                    case EntityState.Modified:
                        entry.Entity.ModifiedDate = now;
                        entry.Entity.ModifiedByUserId = userId;
                        // Prevent changing audit fields on update
                        entry.Property(e => e.CreatedDate).IsModified = false;
                        entry.Property(e => e.CreatedByUserId).IsModified = false;
                        entry.Property(e => e.TenantId).IsModified = false;
                        break;
                }
            }
        }
    }
}
