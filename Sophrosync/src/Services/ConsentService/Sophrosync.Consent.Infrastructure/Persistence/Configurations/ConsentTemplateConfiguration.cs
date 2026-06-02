using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sophrosync.Consent.Domain.Entities;

namespace Sophrosync.Consent.Infrastructure.Persistence.Configurations;

public sealed class ConsentTemplateConfiguration : IEntityTypeConfiguration<ConsentTemplate>
{
    public void Configure(EntityTypeBuilder<ConsentTemplate> builder)
    {
        builder.ToTable("consent_templates");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.Title).HasMaxLength(500).IsRequired();
        builder.Property(e => e.BodyText).HasMaxLength(50000).IsRequired();
        builder.HasIndex(e => new { e.TenantId, e.Purpose, e.Status });
    }
}
