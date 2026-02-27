using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Scheduly.Domain.Entities;

namespace Scheduly.Infrastructure.Persistence.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("tenants");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Name).IsRequired().HasMaxLength(100);
        builder.Property(t => t.Slug).IsRequired().HasMaxLength(100);
        builder.HasIndex(t => t.Slug).IsUnique();
        builder.Ignore(t => t.DomainEvents);
    }
}
