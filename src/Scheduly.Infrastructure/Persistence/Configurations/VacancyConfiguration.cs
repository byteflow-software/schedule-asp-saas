using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Scheduly.Domain.Entities;

namespace Scheduly.Infrastructure.Persistence.Configurations;

public class VacancyConfiguration : IEntityTypeConfiguration<Vacancy>
{
    public void Configure(EntityTypeBuilder<Vacancy> builder)
    {
        builder.ToTable("vacancies");
        builder.HasKey(v => v.Id);
        builder.HasIndex(v => new { v.TenantId, v.UserId, v.StartTime });
        builder.HasIndex(v => new { v.TenantId, v.ServiceId, v.IsBooked });

        builder.HasOne(v => v.Tenant)
            .WithMany(t => t.Vacancies)
            .HasForeignKey(v => v.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(v => v.User)
            .WithMany()
            .HasForeignKey(v => v.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(v => v.Service)
            .WithMany(s => s.Vacancies)
            .HasForeignKey(v => v.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(v => v.Appointment)
            .WithOne(a => a.Vacancy)
            .HasForeignKey<Vacancy>(v => v.AppointmentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Ignore(v => v.DomainEvents);
    }
}
