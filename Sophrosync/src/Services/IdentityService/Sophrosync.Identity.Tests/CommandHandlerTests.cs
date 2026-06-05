using FluentAssertions;
using MockQueryable.Moq;
using Moq;
using Sophrosync.Identity.Application.Commands.RegisterPractice;
using Sophrosync.Identity.Application.Commands.UpdateProfile;
using Sophrosync.Identity.Application.Queries.GetCurrentUser;
using Sophrosync.Identity.Domain.Entities;
using Sophrosync.Identity.Domain.Interfaces;
using Sophrosync.SharedKernel.Abstractions;

namespace Sophrosync.Identity.Tests;

public sealed class CommandHandlerTests
{
    // -----------------------------------------------------------------------
    // RegisterPracticeCommandHandler
    // -----------------------------------------------------------------------

    [Fact]
    public async Task RegisterPractice_ValidCommand_CreatesTenantAndUserProfile()
    {
        var keycloakUserId = Guid.NewGuid();
        var keycloak       = new Mock<IKeycloakAdminService>();
        keycloak.Setup(k => k.CreateUserAsync(It.IsAny<CreateKeycloakUserRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(keycloakUserId);

        var tenantsMock   = new List<Tenant>().BuildMockDbSet();
        var profilesMock  = new List<UserProfile>().BuildMockDbSet();
        var db            = new Mock<IIdentityDbContext>();
        db.Setup(d => d.Tenants).Returns(tenantsMock.Object);
        db.Setup(d => d.UserProfiles).Returns(profilesMock.Object);
        db.Setup(d => d.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new RegisterPracticeCommandHandler(db.Object, keycloak.Object);
        var cmd = new RegisterPracticeCommand(
            "jane@example.com", "Password123!", "Jane", "Doe",
            "Sunflower Practice", "Europe/Kyiv", true);

        var result = await handler.Handle(cmd, CancellationToken.None);

        result.Message.Should().NotBeNullOrEmpty();
        keycloak.Verify(k => k.CreateUserAsync(It.IsAny<CreateKeycloakUserRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        db.Verify(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegisterPractice_KeycloakFails_DoesNotSaveToDb()
    {
        var keycloak = new Mock<IKeycloakAdminService>();
        keycloak.Setup(k => k.CreateUserAsync(It.IsAny<CreateKeycloakUserRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Keycloak error"));

        var tenantsMock  = new List<Tenant>().BuildMockDbSet();
        var profilesMock = new List<UserProfile>().BuildMockDbSet();
        var db           = new Mock<IIdentityDbContext>();
        db.Setup(d => d.Tenants).Returns(tenantsMock.Object);
        db.Setup(d => d.UserProfiles).Returns(profilesMock.Object);

        var handler = new RegisterPracticeCommandHandler(db.Object, keycloak.Object);
        var cmd = new RegisterPracticeCommand(
            "jane@example.com", "Password123!", "Jane", "Doe",
            "Sunflower Practice", "Europe/Kyiv", true);

        var act = () => handler.Handle(cmd, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
        db.Verify(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RegisterPractice_DbFails_DeletesKeycloakUser()
    {
        var keycloakUserId = Guid.NewGuid();
        var keycloak = new Mock<IKeycloakAdminService>();
        keycloak.Setup(k => k.CreateUserAsync(It.IsAny<CreateKeycloakUserRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(keycloakUserId);
        keycloak.Setup(k => k.DeleteUserAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var tenantsMock  = new List<Tenant>().BuildMockDbSet();
        var profilesMock = new List<UserProfile>().BuildMockDbSet();
        var db           = new Mock<IIdentityDbContext>();
        db.Setup(d => d.Tenants).Returns(tenantsMock.Object);
        db.Setup(d => d.UserProfiles).Returns(profilesMock.Object);
        db.Setup(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("DB error"));

        var handler = new RegisterPracticeCommandHandler(db.Object, keycloak.Object);
        var cmd = new RegisterPracticeCommand(
            "jane@example.com", "Password123!", "Jane", "Doe",
            "Sunflower Practice", "Europe/Kyiv", true);

        var act = () => handler.Handle(cmd, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
        keycloak.Verify(k => k.DeleteUserAsync(keycloakUserId, It.IsAny<CancellationToken>()), Times.Once);
    }

    // -----------------------------------------------------------------------
    // GetCurrentUserQueryHandler
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetCurrentUser_ExistingProfile_ReturnsDtoWithTenantName()
    {
        var tenant  = Tenant.Create("Sunflower Practice", "Europe/Kyiv");
        var profile = UserProfile.Create(tenant.Id, Guid.NewGuid(), "Jane", "Doe", "jane@example.com", DateTime.UtcNow);

        var tenantsMock  = new List<Tenant> { tenant }.BuildMockDbSet();
        var profilesMock = new List<UserProfile> { profile }.BuildMockDbSet();

        var db = new Mock<IIdentityDbContext>();
        db.Setup(d => d.Tenants).Returns(tenantsMock.Object);
        db.Setup(d => d.UserProfiles).Returns(profilesMock.Object);

        var currentUser = new Mock<ICurrentUser>();
        currentUser.Setup(u => u.Email).Returns("jane@example.com");

        var handler = new GetCurrentUserQueryHandler(db.Object, currentUser.Object);
        var result  = await handler.Handle(new GetCurrentUserQuery(), CancellationToken.None);

        result.Should().NotBeNull();
        result.FirstName.Should().Be("Jane");
        result.PracticeName.Should().Be("Sunflower Practice");
        result.Email.Should().Be("jane@example.com");
        result.Role.Should().Be("therapist");
    }

    [Fact]
    public async Task GetCurrentUser_MissingProfile_ThrowsKeyNotFoundException()
    {
        var tenantsMock  = new List<Tenant>().BuildMockDbSet();
        var profilesMock = new List<UserProfile>().BuildMockDbSet();

        var db = new Mock<IIdentityDbContext>();
        db.Setup(d => d.Tenants).Returns(tenantsMock.Object);
        db.Setup(d => d.UserProfiles).Returns(profilesMock.Object);

        var currentUser = new Mock<ICurrentUser>();
        currentUser.Setup(u => u.Email).Returns("missing@example.com");

        var handler = new GetCurrentUserQueryHandler(db.Object, currentUser.Object);
        var act     = () => handler.Handle(new GetCurrentUserQuery(), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    // -----------------------------------------------------------------------
    // UpdateProfileCommandHandler
    // -----------------------------------------------------------------------

    [Fact]
    public async Task UpdateProfile_ExistingProfile_UpdatesNameInDbAndKeycloak()
    {
        var userId  = Guid.NewGuid();
        var profile = UserProfile.Create(Guid.NewGuid(), userId, "Jane", "Doe", "jane@example.com", DateTime.UtcNow);

        var profilesMock = new List<UserProfile> { profile }.BuildMockDbSet();
        var db = new Mock<IIdentityDbContext>();
        db.Setup(d => d.UserProfiles).Returns(profilesMock.Object);
        db.Setup(d => d.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var keycloak = new Mock<IKeycloakAdminService>();
        keycloak.Setup(k => k.UpdateUserAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var currentUser = new Mock<ICurrentUser>();
        currentUser.Setup(u => u.Id).Returns(userId);

        var handler = new UpdateProfileCommandHandler(db.Object, keycloak.Object, currentUser.Object);
        var cmd     = new UpdateProfileCommand("Alice", "Smith");

        var result = await handler.Handle(cmd, CancellationToken.None);

        result.FirstName.Should().Be("Alice");
        result.LastName.Should().Be("Smith");
        keycloak.Verify(k => k.UpdateUserAsync(userId, "Alice", "Smith", It.IsAny<CancellationToken>()), Times.Once);
        db.Verify(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateProfile_MissingProfile_ThrowsKeyNotFoundException()
    {
        var profilesMock = new List<UserProfile>().BuildMockDbSet();
        var db = new Mock<IIdentityDbContext>();
        db.Setup(d => d.UserProfiles).Returns(profilesMock.Object);

        var keycloak    = new Mock<IKeycloakAdminService>();
        var currentUser = new Mock<ICurrentUser>();
        currentUser.Setup(u => u.Id).Returns(Guid.NewGuid());

        var handler = new UpdateProfileCommandHandler(db.Object, keycloak.Object, currentUser.Object);
        var act     = () => handler.Handle(new UpdateProfileCommand("Alice", "Smith"), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
