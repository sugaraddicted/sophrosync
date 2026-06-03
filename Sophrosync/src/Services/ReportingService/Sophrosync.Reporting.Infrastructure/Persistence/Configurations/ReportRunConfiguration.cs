using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sophrosync.Reporting.Domain.Entities;
using Sophrosync.SharedKernel.Security;

namespace Sophrosync.Reporting.Infrastructure.Persistence.Configurations;

public sealed class ReportRunConfiguration : IEntityTypeConfiguration<ReportRun>
{
    private readonly string _encryptionKey;

    public ReportRunConfiguration(string encryptionKey) => _encryptionKey = encryptionKey;

    // Parameterless ctor for EF design-time
    public ReportRunConfiguration() : this("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=") { }

    public void Configure(EntityTypeBuilder<ReportRun> builder)
    {
        builder.ToTable("report_runs");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.TenantId).IsRequired();

        builder.Property(e => e.ResultJson)
            .HasConversion(new EncryptedStringConverter(_encryptionKey));

        builder.Property(e => e.FailureReason).HasMaxLength(2000);
        builder.HasIndex(e => new { e.TenantId, e.ReportDefinitionId, e.Status });
    }
}
