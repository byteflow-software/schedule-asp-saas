using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Scheduly.Domain.Entities;

namespace Scheduly.Infrastructure.Persistence.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Token).IsRequired().HasMaxLength(500);
        builder.HasIndex(r => r.Token);
        builder.HasIndex(r => r.UserId);
        builder.Property(r => r.CreatedByIp).HasMaxLength(50);

        builder.HasOne(r => r.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(r => r.IsExpired);
        builder.Ignore(r => r.IsRevoked);
        builder.Ignore(r => r.IsActive);
        builder.Ignore(r => r.DomainEvents);
    }
}
