using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sophrosync.Consent.Domain.Entities;
using Sophrosync.SharedKernel.Security;

namespace Sophrosync.Consent.Infrastructure.Persistence.Configurations;

public sealed class ConsentRecordConfiguration : IEntityTypeConfiguration<ConsentRecord>
{
    private readonly string _encryptionKey;

    public ConsentRecordConfiguration(string encryptionKey)
    {
        _encryptionKey = encryptionKey;
    }

    public ConsentRecordConfiguration() : this("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=") { }

    public void Configure(EntityTypeBuilder<ConsentRecord> builder)
    {
        builder.ToTable("consent_records");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.ClientId).IsRequired();

        builder.Property(e => e.IpAddress)
            .HasMaxLength(500)
            .HasConversion(new EncryptedStringConverter(_encryptionKey))
            .IsRequired();

        builder.Property(e => e.TemplateBodySnapshot)
            .HasMaxLength(100000)
            .HasConversion(new EncryptedStringConverter(_encryptionKey))
            .IsRequired();

        builder.HasIndex(e => new { e.TenantId, e.ClientId, e.Purpose });
        builder.HasIndex(e => new { e.ClientId, e.CreatedAt });
    }
}
