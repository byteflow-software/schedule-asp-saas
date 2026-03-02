using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Scheduly.Domain.Entities;

namespace Scheduly.Infrastructure.Persistence.Configurations;

public class ErrorLogConfiguration : IEntityTypeConfiguration<ErrorLog>
{
    public void Configure(EntityTypeBuilder<ErrorLog> builder)
    {
        builder.ToTable("error_logs");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Timestamp).IsRequired();
        builder.Property(e => e.Level).HasMaxLength(20).IsRequired();
        builder.Property(e => e.Message).HasMaxLength(2000).IsRequired();
        builder.Property(e => e.ExceptionType).HasMaxLength(500);
        builder.Property(e => e.StackTrace).HasMaxLength(8000);
        builder.Property(e => e.Source).HasMaxLength(200);

        builder.Property(e => e.RequestPath).HasMaxLength(500);
        builder.Property(e => e.RequestMethod).HasMaxLength(10);
        builder.Property(e => e.RequestBody).HasMaxLength(4000);

        builder.Property(e => e.ExternalRequestUrl).HasMaxLength(1000);
        builder.Property(e => e.ExternalResponseBody).HasMaxLength(4000);

        builder.HasIndex(e => e.Timestamp);
        builder.HasIndex(e => e.Level);
        builder.HasIndex(e => e.TenantId);
    }
}
