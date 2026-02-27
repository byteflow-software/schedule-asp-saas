using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Scheduly.Domain.Entities;

namespace Scheduly.Infrastructure.Persistence.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("transactions");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.ReferenceNumber).HasMaxLength(50).IsRequired();
        builder.Property(t => t.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(t => t.Type).HasConversion<string>().HasMaxLength(20);
        builder.Property(t => t.Description).HasMaxLength(500);
        builder.Property(t => t.PaymentMethod).HasMaxLength(50);
        builder.HasIndex(t => new { t.TenantId, t.ReferenceNumber }).IsUnique();
        builder.HasIndex(t => new { t.TenantId, t.Status });

        builder.HasOne(t => t.Tenant)
            .WithMany(te => te.Transactions)
            .HasForeignKey(t => t.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Appointment)
            .WithOne(a => a.Transaction)
            .HasForeignKey<Transaction>(t => t.AppointmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Customer)
            .WithMany()
            .HasForeignKey(t => t.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(t => t.DomainEvents);
    }
}
