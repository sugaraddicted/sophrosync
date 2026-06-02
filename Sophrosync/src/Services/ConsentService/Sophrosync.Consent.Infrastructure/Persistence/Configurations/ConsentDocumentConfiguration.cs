using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sophrosync.Consent.Domain.Entities;

namespace Sophrosync.Consent.Infrastructure.Persistence.Configurations;

public sealed class ConsentDocumentConfiguration : IEntityTypeConfiguration<ConsentDocument>
{
    public void Configure(EntityTypeBuilder<ConsentDocument> builder)
    {
        builder.ToTable("consent_documents");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.TenantId).IsRequired();
        builder.Property(d => d.ConsentRecordId).IsRequired();
        builder.Property(d => d.FileName).HasMaxLength(500).IsRequired();
        builder.Property(d => d.ContentType).HasMaxLength(200).IsRequired();
        builder.Property(d => d.SizeBytes).IsRequired();
        builder.Property(d => d.Data).IsRequired();
        builder.Property(d => d.CreatedAt).HasColumnType("timestamptz").IsRequired();
        builder.Property(d => d.UpdatedAt).HasColumnType("timestamptz").IsRequired();
        builder.HasIndex(d => d.ConsentRecordId).IsUnique();
        builder.HasIndex(d => d.TenantId);
    }
}
