using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Scheduly.Domain.Entities;

namespace Scheduly.Infrastructure.Persistence.Configurations;

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.ToTable("appointments");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Notes).HasMaxLength(500);
        builder.Property(a => a.Status).HasConversion<string>().HasMaxLength(20);
        builder.HasIndex(a => new { a.TenantId, a.UserId, a.StartTime, a.EndTime });
        builder.HasIndex(a => new { a.TenantId, a.StartTime });

        builder.HasOne(a => a.Tenant)
            .WithMany(t => t.Appointments)
            .HasForeignKey(a => a.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Customer)
            .WithMany(c => c.Appointments)
            .HasForeignKey(a => a.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.User)
            .WithMany(u => u.Appointments)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Service)
            .WithMany(s => s.Appointments)
            .HasForeignKey(a => a.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(a => a.DomainEvents);
    }
}
