using FluentAssertions;
using Sophrosync.Identity.Domain.Entities;

namespace Sophrosync.Identity.Tests;

public sealed class UserProfileEntityTests
{
    private static readonly Guid TenantId = Guid.NewGuid();

    // -----------------------------------------------------------------------
    // UserProfile.Create
    // -----------------------------------------------------------------------

    [Fact]
    public void Create_WithValidArgs_SetsTherapistRole()
    {
        var profile = UserProfile.Create(
            TenantId, Guid.NewGuid(), "Jane", "Doe", "jane@example.com", DateTime.UtcNow);

        profile.Role.Should().Be("therapist");
        profile.TenantId.Should().Be(TenantId);
        profile.FirstName.Should().Be("Jane");
        profile.LastName.Should().Be("Doe");
        profile.Email.Should().Be("jane@example.com");
        profile.IsDeleted.Should().BeFalse();
        profile.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void UpdateName_ValidNames_UpdatesFirstAndLastName()
    {
        var profile = UserProfile.Create(
            TenantId, Guid.NewGuid(), "Jane", "Doe", "jane@example.com", DateTime.UtcNow);

        profile.UpdateName("Alice", "Smith");

        profile.FirstName.Should().Be("Alice");
        profile.LastName.Should().Be("Smith");
    }

    // -----------------------------------------------------------------------
    // Tenant.Create
    // -----------------------------------------------------------------------

    [Fact]
    public void Tenant_Create_WithValidArgs_IsActive()
    {
        var tenant = Tenant.Create("Sunflower Practice", "Europe/Kyiv");

        tenant.Name.Should().Be("Sunflower Practice");
        tenant.TimeZone.Should().Be("Europe/Kyiv");
        tenant.IsActive.Should().BeTrue();
        tenant.IsDeleted.Should().BeFalse();
        tenant.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Tenant_Create_WithEmptyName_ThrowsArgumentException()
    {
        var act = () => Tenant.Create("", "Europe/Kyiv");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Tenant_Create_WithEmptyTimeZone_ThrowsArgumentException()
    {
        var act = () => Tenant.Create("Practice Name", "");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Tenant_SoftDelete_SetsIsDeletedTrue()
    {
        var tenant = Tenant.Create("Practice", "Europe/Kyiv");

        tenant.SoftDelete();

        tenant.IsDeleted.Should().BeTrue();
        tenant.DeletedAt.Should().NotBeNull();
    }
}
