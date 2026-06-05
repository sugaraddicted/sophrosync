using FluentAssertions;
using Moq;
using Sophrosync.Clients.Application.Commands.CreateClient;
using Sophrosync.Clients.Application.Commands.DeleteClient;
using Sophrosync.Clients.Application.Commands.UpdateClient;
using Sophrosync.Clients.Application.Queries.GetClientById;
using Sophrosync.Clients.Application.Queries.GetClients;
using Sophrosync.Clients.Domain.Entities;
using Sophrosync.Clients.Domain.Interfaces;
using Sophrosync.SharedKernel.Abstractions;

namespace Sophrosync.Clients.Tests;

public sealed class CommandHandlerTests
{
    private static readonly Guid TenantId = Guid.NewGuid();

    private static Mock<ICurrentTenant> ValidTenant()
    {
        var mock = new Mock<ICurrentTenant>();
        mock.Setup(t => t.Id).Returns(TenantId);
        mock.Setup(t => t.HasTenant).Returns(true);
        return mock;
    }

    private static Client BuildClient() =>
        Client.Create(TenantId, "Jane Doe", "jane@example.com", "+380501234567");

    // -----------------------------------------------------------------------
    // CreateClientCommandHandler
    // -----------------------------------------------------------------------

    [Fact]
    public async Task CreateHandler_ValidCommand_CallsAddAndSave()
    {
        var repo   = new Mock<IClientRepository>();
        var tenant = ValidTenant();

        var handler = new CreateClientCommandHandler(repo.Object, tenant.Object);
        var cmd     = new CreateClientCommand("Jane Doe", "jane@example.com", "+380501234567");

        var dto = await handler.Handle(cmd, CancellationToken.None);

        dto.Should().NotBeNull();
        dto.Name.Should().Be("Jane Doe");
        dto.Status.Should().Be(ClientStatus.Active);
        repo.Verify(r => r.AddAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()), Times.Once);
        repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateHandler_ValidCommand_ReturnsNewGuid()
    {
        var repo   = new Mock<IClientRepository>();
        var tenant = ValidTenant();

        var handler = new CreateClientCommandHandler(repo.Object, tenant.Object);
        var cmd     = new CreateClientCommand("Test Client", "test@example.com", "");

        var dto = await handler.Handle(cmd, CancellationToken.None);

        dto.Id.Should().NotBeEmpty();
    }

    // -----------------------------------------------------------------------
    // UpdateClientCommandHandler
    // -----------------------------------------------------------------------

    [Fact]
    public async Task UpdateHandler_ExistingClient_UpdatesAndReturnsDto()
    {
        var client = BuildClient();
        var repo   = new Mock<IClientRepository>();
        repo.Setup(r => r.GetByIdAsync(client.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(client);

        var handler = new UpdateClientCommandHandler(repo.Object);
        var cmd     = new UpdateClientCommand(client.Id, "Updated Name", "updated@example.com", "", ClientStatus.Inactive);

        var dto = await handler.Handle(cmd, CancellationToken.None);

        dto.Should().NotBeNull();
        dto!.Name.Should().Be("Updated Name");
        dto.Status.Should().Be(ClientStatus.Inactive);
        repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateHandler_NonExistentId_ReturnsNull()
    {
        var repo = new Mock<IClientRepository>();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Client?)null);

        var handler = new UpdateClientCommandHandler(repo.Object);
        var cmd     = new UpdateClientCommand(Guid.NewGuid(), "Name", "email@example.com", "", ClientStatus.Active);

        var dto = await handler.Handle(cmd, CancellationToken.None);

        dto.Should().BeNull();
    }

    // -----------------------------------------------------------------------
    // DeleteClientCommandHandler
    // -----------------------------------------------------------------------

    [Fact]
    public async Task DeleteHandler_ExistingClient_SoftDeletesAndReturnsTrue()
    {
        var client = BuildClient();
        var repo   = new Mock<IClientRepository>();
        repo.Setup(r => r.GetByIdAsync(client.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(client);

        var handler = new DeleteClientCommandHandler(repo.Object);
        var result  = await handler.Handle(new DeleteClientCommand(client.Id), CancellationToken.None);

        result.Should().BeTrue();
        client.IsDeleted.Should().BeTrue();
        repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteHandler_NonExistentId_ReturnsFalse()
    {
        var repo = new Mock<IClientRepository>();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Client?)null);

        var handler = new DeleteClientCommandHandler(repo.Object);
        var result  = await handler.Handle(new DeleteClientCommand(Guid.NewGuid()), CancellationToken.None);

        result.Should().BeFalse();
    }

    // -----------------------------------------------------------------------
    // GetClientByIdQueryHandler
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetByIdHandler_ExistingClient_ReturnsMappedDto()
    {
        var client = BuildClient();
        var repo   = new Mock<IClientRepository>();
        repo.Setup(r => r.GetByIdAsync(client.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(client);

        var handler = new GetClientByIdQueryHandler(repo.Object);
        var dto     = await handler.Handle(new GetClientByIdQuery(client.Id), CancellationToken.None);

        dto.Should().NotBeNull();
        dto!.Id.Should().Be(client.Id);
        dto.Name.Should().Be("Jane Doe");
        dto.Email.Should().Be("jane@example.com");
    }

    [Fact]
    public async Task GetByIdHandler_NonExistentId_ReturnsNull()
    {
        var repo = new Mock<IClientRepository>();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Client?)null);

        var handler = new GetClientByIdQueryHandler(repo.Object);
        var dto     = await handler.Handle(new GetClientByIdQuery(Guid.NewGuid()), CancellationToken.None);

        dto.Should().BeNull();
    }

    // -----------------------------------------------------------------------
    // GetClientsQueryHandler
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetClientsHandler_ReturnsAllClientsForTenant()
    {
        var clients = new List<Client>
        {
            Client.Create(TenantId, "Alice", "alice@example.com", ""),
            Client.Create(TenantId, "Bob", "bob@example.com", "")
        };
        var repo = new Mock<IClientRepository>();
        repo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(clients.AsReadOnly());

        var handler = new GetClientsQueryHandler(repo.Object);
        var result  = await handler.Handle(new GetClientsQuery(), CancellationToken.None);

        result.Should().HaveCount(2);
        result[0].Name.Should().Be("Alice");
        result[1].Name.Should().Be("Bob");
    }
}
