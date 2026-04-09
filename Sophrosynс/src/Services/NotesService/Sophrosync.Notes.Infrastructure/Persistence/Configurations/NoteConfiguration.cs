using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sophrosync.Notes.Domain.Entities;
using Sophrosync.SharedKernel.Security;

namespace Sophrosync.Notes.Infrastructure.Persistence.Configurations;

public sealed class NoteConfiguration : IEntityTypeConfiguration<Note>
{
    private readonly string _encryptionKey;

    public NoteConfiguration(string encryptionKey)
    {
        _encryptionKey = encryptionKey;
    }

    public void Configure(EntityTypeBuilder<Note> builder)
    {
        builder.ToTable("notes");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.ClientId).IsRequired();
        builder.Property(e => e.AppointmentId);
        builder.Property(e => e.TherapistId).IsRequired();

        builder.Property(e => e.AuthorFullName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.Type)
            .HasMaxLength(20)
            .IsRequired();

        // PHI fields — transparent AES-256-GCM encryption via EF Core value converter
        // Title: validator allows up to 200 plaintext chars. AES-256-GCM adds a 12-byte nonce,
        // 16-byte tag, and base64 overhead (~4/3 ratio), giving ~295 chars max — 500 is sufficient.
        builder.Property(e => e.Title)
            .HasMaxLength(500)
            .HasConversion(new EncryptedStringConverter(_encryptionKey));

        // Content: validator allows up to 50 000 plaintext chars. AES-256-GCM + base64 overhead
        // expands this to roughly 67 000 chars — 70 000 provides a safe margin.
        builder.Property(e => e.Content)
            .HasMaxLength(70000)
            .IsRequired()
            .HasConversion(new EncryptedStringConverter(_encryptionKey));

        builder.Property(e => e.Tags)
            .HasMaxLength(500);

        builder.Property(e => e.Status)
            .HasMaxLength(25)
            .IsRequired()
            .HasDefaultValue(NoteStatus.Draft);

        builder.Property(e => e.SignedAt)
            .HasColumnType("timestamptz");

        builder.Property(e => e.SignedByUserId);

        builder.Property(e => e.SignedByFullName)
            .HasMaxLength(200);

        builder.Property(e => e.LockedAt)
            .HasColumnType("timestamptz");

        builder.Property(e => e.LockedByUserId);

        builder.Property(e => e.LockedByFullName)
            .HasMaxLength(200);

        // Self-referencing amendment link — stored as a plain nullable FK column, no navigation required.
        builder.Property(e => e.AmendedFromId);

        builder.Property(e => e.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(e => e.DeletedAt)
            .HasColumnType("timestamptz");

        builder.Property(e => e.DeletedByUserId);

        builder.Property(e => e.DeletedByFullName)
            .HasMaxLength(200);

        // Audit timestamps
        builder.Property(e => e.CreatedAt)
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(e => e.UpdatedAt)
            .HasColumnType("timestamptz")
            .IsRequired();

        // Composite indexes for common multi-tenant query patterns
        builder.HasIndex(e => new { e.TenantId, e.ClientId });
        builder.HasIndex(e => new { e.TenantId, e.TherapistId });
        builder.HasIndex(e => new { e.TenantId, e.Status });
    }
}
