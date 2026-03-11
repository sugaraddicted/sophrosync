using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sophrosync.Notifications.Domain.Entities;
using Sophrosync.SharedKernel.Security;

namespace Sophrosync.Notifications.Infrastructure.Persistence.Configurations;

public sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    private readonly string _encryptionKey;

    public NotificationConfiguration(string encryptionKey)
    {
        _encryptionKey = encryptionKey;
    }

    // Parameterless ctor for EF design-time tools
    public NotificationConfiguration() : this("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=") { }

    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notifications");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.RecipientUserId).IsRequired();
        builder.Property(e => e.Subject).HasMaxLength(500).IsRequired();
        builder.Property(e => e.Body)
            .HasMaxLength(20000)
            .HasConversion(new EncryptedStringConverter(_encryptionKey))
            .IsRequired();
        builder.Property(e => e.CorrelationId).HasMaxLength(200).IsRequired();
        builder.Property(e => e.FailureReason).HasMaxLength(2000);
        builder.HasIndex(e => e.CorrelationId).IsUnique();
        builder.HasIndex(e => new { e.TenantId, e.RecipientUserId, e.Status });
        builder.HasIndex(e => new { e.Status, e.ScheduledFor });
    }
}
