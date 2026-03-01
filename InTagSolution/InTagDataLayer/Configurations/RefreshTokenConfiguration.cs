using InTagEntitiesLayer.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace InTagDataLayer.Configurations
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.HasKey(r => r.Id);

            builder.Property(r => r.Token)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(r => r.UserId).IsRequired();
            builder.Property(r => r.TenantId).IsRequired();
            builder.Property(r => r.CreatedDate).IsRequired();
            builder.Property(r => r.ExpiresDate).IsRequired();
            builder.Property(r => r.ReplacedByToken).HasMaxLength(500);

            builder.HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(r => r.Token)
                .IsUnique()
                .HasDatabaseName("IX_RefreshToken_Token");

            builder.HasIndex(r => new { r.UserId, r.TenantId })
                .HasDatabaseName("IX_RefreshToken_UserId_TenantId");
        }
    }
}
