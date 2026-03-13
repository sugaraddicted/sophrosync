using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Sophrosync.SharedKernel.Services;
using Xunit;

namespace Sophrosync.SharedKernel.Tests.Services;

public sealed class CurrentTenantServiceTests
{
    private const string TenantIdClaimType = "tenant_id";

    private static ClaimsPrincipal AuthenticatedPrincipalWithTenant(Guid tenantId)
    {
        var identity = new ClaimsIdentity(
            [new Claim(TenantIdClaimType, tenantId.ToString())],
            authenticationType: "TestAuth");
        return new ClaimsPrincipal(identity);
    }

    private static ClaimsPrincipal AuthenticatedPrincipalWithoutTenant()
    {
        var identity = new ClaimsIdentity([], authenticationType: "TestAuth");
        return new ClaimsPrincipal(identity);
    }

    private static ClaimsPrincipal UnauthenticatedPrincipal()
    {
        // No authenticationType = not authenticated
        var identity = new ClaimsIdentity();
        return new ClaimsPrincipal(identity);
    }

    private static CurrentTenantService CreateService(IHttpContextAccessor accessor)
        => new(accessor, NullLogger<CurrentTenantService>.Instance);

    [Fact]
    public void Id_WhenValidJwtClaim_ReturnsExpectedGuid()
    {
        var expected = Guid.NewGuid();
        var context = new DefaultHttpContext
        {
            User = AuthenticatedPrincipalWithTenant(expected)
        };
        var accessor = Substitute.For<IHttpContextAccessor>();
        accessor.HttpContext.Returns(context);

        var service = CreateService(accessor);

        service.Id.Should().Be(expected);
    }

    [Fact]
    public void Id_WhenNoHttpContext_ReturnsGuidEmpty()
    {
        var accessor = Substitute.For<IHttpContextAccessor>();
        accessor.HttpContext.Returns((HttpContext?)null);

        var service = CreateService(accessor);

        service.Id.Should().Be(Guid.Empty);
    }

    [Fact]
    public void Id_WhenMissingTenantIdClaim_ReturnsGuidEmpty()
    {
        var context = new DefaultHttpContext
        {
            User = AuthenticatedPrincipalWithoutTenant()
        };
        var accessor = Substitute.For<IHttpContextAccessor>();
        accessor.HttpContext.Returns(context);

        var service = CreateService(accessor);

        service.Id.Should().Be(Guid.Empty);
    }

    [Fact]
    public void HasTenant_WhenAuthenticatedWithValidTenantClaim_ReturnsTrue()
    {
        var context = new DefaultHttpContext
        {
            User = AuthenticatedPrincipalWithTenant(Guid.NewGuid())
        };
        var accessor = Substitute.For<IHttpContextAccessor>();
        accessor.HttpContext.Returns(context);

        var service = CreateService(accessor);

        service.HasTenant.Should().BeTrue();
    }

    [Fact]
    public void HasTenant_WhenNotAuthenticated_ReturnsFalse()
    {
        var context = new DefaultHttpContext
        {
            User = UnauthenticatedPrincipal()
        };
        var accessor = Substitute.For<IHttpContextAccessor>();
        accessor.HttpContext.Returns(context);

        var service = CreateService(accessor);

        service.HasTenant.Should().BeFalse();
    }

    [Fact]
    public void HasTenant_WhenAuthenticatedButNoTenantClaim_ReturnsFalse()
    {
        var context = new DefaultHttpContext
        {
            User = AuthenticatedPrincipalWithoutTenant()
        };
        var accessor = Substitute.For<IHttpContextAccessor>();
        accessor.HttpContext.Returns(context);

        var service = CreateService(accessor);

        service.HasTenant.Should().BeFalse();
    }

    [Fact]
    public void Id_WhenMalformedTenantIdClaim_ReturnsGuidEmpty()
    {
        var identity = new ClaimsIdentity(
            [new Claim(TenantIdClaimType, "not-a-guid")],
            authenticationType: "TestAuth");
        var context = new DefaultHttpContext { User = new ClaimsPrincipal(identity) };
        var accessor = Substitute.For<IHttpContextAccessor>();
        accessor.HttpContext.Returns(context);

        var service = CreateService(accessor);

        service.Id.Should().Be(Guid.Empty);
    }

    [Fact]
    public void HasTenant_WhenNoHttpContext_ReturnsFalse()
    {
        var accessor = Substitute.For<IHttpContextAccessor>();
        accessor.HttpContext.Returns((HttpContext?)null);

        var service = CreateService(accessor);

        service.HasTenant.Should().BeFalse();
    }

    [Fact]
    public void HasTenant_WhenMalformedTenantClaim_ReturnsFalse()
    {
        var identity = new ClaimsIdentity(
            [new Claim(TenantIdClaimType, "not-a-guid")],
            authenticationType: "TestAuth");
        var context = new DefaultHttpContext { User = new ClaimsPrincipal(identity) };
        var accessor = Substitute.For<IHttpContextAccessor>();
        accessor.HttpContext.Returns(context);

        var service = CreateService(accessor);

        service.HasTenant.Should().BeFalse();
    }
}
