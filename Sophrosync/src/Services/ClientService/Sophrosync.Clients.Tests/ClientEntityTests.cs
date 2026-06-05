using FluentAssertions;
using Sophrosync.Clients.Domain.Entities;

namespace Sophrosync.Clients.Tests;

public sealed class ClientEntityTests
{
    private static readonly Guid TenantId = Guid.NewGuid();

    // -----------------------------------------------------------------------
    // Create — happy path
    // -----------------------------------------------------------------------

    [Fact]
    public void Create_WithValidArgs_ReturnsActiveClient()
    {
        var client = Client.Create(TenantId, "Jane Doe", "jane@example.com", "+380501234567");

        client.TenantId.Should().Be(TenantId);
        client.Name.Should().Be("Jane Doe");
        client.Email.Should().Be("jane@example.com");
        client.Phone.Should().Be("+380501234567");
        client.Status.Should().Be(ClientStatus.Active);
        client.IsDeleted.Should().BeFalse();
        client.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Create_WithEmptyPhone_SetsEmptyString()
    {
        var client = Client.Create(TenantId, "Jane Doe", "jane@example.com", null!);

        client.Phone.Should().BeEmpty();
    }

    // -----------------------------------------------------------------------
    // Create — argument guards
    // -----------------------------------------------------------------------

    [Fact]
    public void Create_WithEmptyName_ThrowsArgumentException()
    {
        var act = () => Client.Create(TenantId, "", "jane@example.com", null!);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithWhitespaceName_ThrowsArgumentException()
    {
        var act = () => Client.Create(TenantId, "   ", "jane@example.com", null!);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithEmptyEmail_ThrowsArgumentException()
    {
        var act = () => Client.Create(TenantId, "Jane Doe", "", null!);

        act.Should().Throw<ArgumentException>();
    }

    // -----------------------------------------------------------------------
    // Update — happy path
    // -----------------------------------------------------------------------

    [Fact]
    public void Update_WithValidArgs_UpdatesAllFields()
    {
        var client = Client.Create(TenantId, "Jane Doe", "jane@example.com", "");

        client.Update("John Smith", "john@example.com", "+1234567890", ClientStatus.Inactive);

        client.Name.Should().Be("John Smith");
        client.Email.Should().Be("john@example.com");
        client.Phone.Should().Be("+1234567890");
        client.Status.Should().Be(ClientStatus.Inactive);
    }

    [Fact]
    public void Update_WithInvalidStatus_ThrowsArgumentException()
    {
        var client = Client.Create(TenantId, "Jane Doe", "jane@example.com", "");

        var act = () => client.Update("Jane Doe", "jane@example.com", "", "discharged");

        act.Should().Throw<ArgumentException>().WithMessage("*discharged*");
    }

    // -----------------------------------------------------------------------
    // SoftDelete — happy path
    // -----------------------------------------------------------------------

    [Fact]
    public void SoftDelete_SetsIsDeletedTrue()
    {
        var client = Client.Create(TenantId, "Jane Doe", "jane@example.com", "");

        client.SoftDelete();

        client.IsDeleted.Should().BeTrue();
        client.DeletedAt.Should().NotBeNull();
    }
}
