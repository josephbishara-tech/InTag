using InTagEntitiesLayer.Common;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace InTagDataLayer.Context
{
    /// <summary>
    /// Shared catalog database — stores tenant metadata, subscription status,
    /// and feature flags. Separate from the per-tenant InTagDbContext.
    /// </summary>
    public class CatalogDbContext : DbContext
    {
        public CatalogDbContext(DbContextOptions<CatalogDbContext> options)
            : base(options) { }

        public DbSet<Tenant> Tenants => Set<Tenant>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Tenant>(builder =>
            {
                builder.HasKey(t => t.Id);

                builder.Property(t => t.Subdomain)
                    .IsRequired()
                    .HasMaxLength(63);

                builder.Property(t => t.CustomDomain)
                    .HasMaxLength(253);

                builder.Property(t => t.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                builder.Property(t => t.FeatureFlags)
                    .IsRequired()
                    .HasDefaultValue("{}");

                builder.Property(t => t.Tier)
                    .IsRequired();

                builder.Property(t => t.Status)
                    .IsRequired();

                // Unique subdomain
                builder.HasIndex(t => t.Subdomain)
                    .IsUnique()
                    .HasDatabaseName("IX_Tenant_Subdomain");

                // Unique custom domain (when set)
                builder.HasIndex(t => t.CustomDomain)
                    .IsUnique()
                    .HasFilter("[CustomDomain] IS NOT NULL")
                    .HasDatabaseName("IX_Tenant_CustomDomain");
            });
        }
    }
}
