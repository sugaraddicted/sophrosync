using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sophrosync.Schedule.Domain.Entities;

namespace Sophrosync.Schedule.Infrastructure.Persistence.Configurations;

public sealed class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.ToTable("appointments");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.TenantId).IsRequired();
        builder.Property(a => a.ClientId).IsRequired();
        builder.Property(a => a.TherapistId).IsRequired();

        builder.Property(a => a.ScheduledAt)
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(a => a.DurationMinutes).IsRequired();

        builder.Property(a => a.Type)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(a => a.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(a => a.Notes).HasMaxLength(10000);
        builder.Property(a => a.CancellationReason).HasMaxLength(500);

        builder.Property(a => a.CreatedAt)
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(a => a.UpdatedAt)
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.HasIndex(a => new { a.TenantId, a.ScheduledAt });
        builder.HasIndex(a => new { a.TenantId, a.ClientId });
        builder.HasIndex(a => new { a.TenantId, a.TherapistId });
    }
}
