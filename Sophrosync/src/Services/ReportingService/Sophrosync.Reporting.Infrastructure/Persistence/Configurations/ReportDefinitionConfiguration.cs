using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sophrosync.Reporting.Domain.Entities;

namespace Sophrosync.Reporting.Infrastructure.Persistence.Configurations;

public sealed class ReportDefinitionConfiguration : IEntityTypeConfiguration<ReportDefinition>
{
    public void Configure(EntityTypeBuilder<ReportDefinition> builder)
    {
        builder.ToTable("report_definitions");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.Name).HasMaxLength(200).IsRequired();

        builder.OwnsOne(e => e.Schedule, schedule =>
        {
            schedule.Property(s => s.IsScheduled).HasColumnName("schedule_is_active");
            schedule.Property(s => s.DayOfWeek).HasColumnName("schedule_day_of_week");
            schedule.Property(s => s.TimeOfDay).HasColumnName("schedule_time_of_day");
        });

        builder.HasIndex(e => new { e.TenantId, e.Type, e.IsActive });
    }
}
