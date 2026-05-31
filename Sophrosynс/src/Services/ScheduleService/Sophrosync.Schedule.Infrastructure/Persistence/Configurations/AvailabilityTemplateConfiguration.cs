using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sophrosync.Schedule.Domain.Entities;

namespace Sophrosync.Schedule.Infrastructure.Persistence.Configurations;

public sealed class AvailabilityTemplateConfiguration : IEntityTypeConfiguration<AvailabilityTemplate>
{
    public void Configure(EntityTypeBuilder<AvailabilityTemplate> builder)
    {
        builder.ToTable("availability_templates");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.TenantId).IsRequired();
        builder.Property(t => t.TherapistId).IsRequired();

        builder.Property(t => t.DayOfWeek)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(t => t.StartTime)
            .HasColumnType("time")
            .IsRequired();

        builder.Property(t => t.EndTime)
            .HasColumnType("time")
            .IsRequired();

        builder.Property(t => t.IsActive).IsRequired();

        builder.Property(t => t.CreatedAt)
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(t => t.UpdatedAt)
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.HasIndex(t => new { t.TenantId, t.TherapistId, t.DayOfWeek })
            .HasDatabaseName("IX_availability_templates_TenantId_TherapistId_DayOfWeek");
    }
}
