using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sophrosync.Clients.Domain.Entities;
using Sophrosync.SharedKernel.Security;

namespace Sophrosync.Clients.Infrastructure.Persistence.Configurations;

public sealed class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    private readonly string _encryptionKey;

    public ClientConfiguration(string encryptionKey)
    {
        _encryptionKey = encryptionKey;
    }

    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.ToTable("clients");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();

        // PHI fields — transparent AES-256-GCM encryption via EF Core value converter
        builder.Property(e => e.Name)
            .HasMaxLength(500)
            .IsRequired()
            .HasConversion(new EncryptedStringConverter(_encryptionKey));

        builder.Property(e => e.Email)
            .HasMaxLength(700)
            .IsRequired()
            .HasConversion(new EncryptedStringConverter(_encryptionKey));

        builder.Property(e => e.Phone)
            .HasMaxLength(200)
            .HasConversion(new EncryptedStringConverter(_encryptionKey));

        builder.Property(e => e.Status)
            .HasMaxLength(20)
            .IsRequired();

        // Audit timestamps
        builder.Property(e => e.CreatedAt)
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(e => e.UpdatedAt)
            .HasColumnType("timestamptz")
            .IsRequired();

        // Soft-delete fields
        builder.Property(e => e.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(e => e.DeletedAt)
            .HasColumnType("timestamptz");

        builder.HasIndex(e => new { e.TenantId, e.Status });
    }
}
