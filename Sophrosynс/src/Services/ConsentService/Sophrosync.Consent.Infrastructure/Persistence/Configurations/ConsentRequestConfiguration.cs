using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sophrosync.Consent.Domain.Entities;

namespace Sophrosync.Consent.Infrastructure.Persistence.Configurations;

public sealed class ConsentRequestConfiguration : IEntityTypeConfiguration<ConsentRequest>
{
    public void Configure(EntityTypeBuilder<ConsentRequest> builder)
    {
        builder.ToTable("consent_requests");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.ClientId).IsRequired();
        builder.HasIndex(e => new { e.TenantId, e.ClientId, e.Status });
        builder.HasIndex(e => new { e.Status, e.ExpiresAt });
    }
}
