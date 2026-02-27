using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Scheduly.Domain.Entities;

namespace Scheduly.Infrastructure.Persistence.Configurations;

public class ServiceConfiguration : IEntityTypeConfiguration<Service>
{
    public void Configure(EntityTypeBuilder<Service> builder)
    {
        builder.ToTable("services");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Name).HasMaxLength(200).IsRequired();
        builder.Property(s => s.Description).HasMaxLength(500);
        builder.HasIndex(s => new { s.TenantId, s.Name }).IsUnique();

        builder.HasOne(s => s.Tenant)
            .WithMany(t => t.Services)
            .HasForeignKey(s => s.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(s => s.DomainEvents);
    }
}
