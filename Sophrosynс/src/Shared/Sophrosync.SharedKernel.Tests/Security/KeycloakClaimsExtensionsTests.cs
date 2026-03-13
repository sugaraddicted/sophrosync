using System.Security.Claims;
using FluentAssertions;
using Sophrosync.SharedKernel.Security;
using Xunit;

namespace Sophrosync.SharedKernel.Tests.Security;

public sealed class KeycloakClaimsExtensionsTests
{
    private const string TenantIdClaimType = "tenant_id";

    private static ClaimsPrincipal PrincipalWithClaim(string claimType, string claimValue)
    {
        var identity = new ClaimsIdentity(
            [new Claim(claimType, claimValue)],
            authenticationType: "TestAuth");
        return new ClaimsPrincipal(identity);
    }

    private static ClaimsPrincipal PrincipalWithoutClaim()
    {
        var identity = new ClaimsIdentity([], authenticationType: "TestAuth");
        return new ClaimsPrincipal(identity);
    }

    [Fact]
    public void TryGetTenantId_WhenClaimPresent_ReturnsTrueAndGuid()
    {
        var expected = Guid.NewGuid();
        var principal = PrincipalWithClaim(TenantIdClaimType, expected.ToString());

        var result = principal.TryGetTenantId(out var tenantId);

        result.Should().BeTrue();
        tenantId.Should().Be(expected);
    }

    [Fact]
    public void TryGetTenantId_WhenClaimMissing_ReturnsFalseAndEmpty()
    {
        var principal = PrincipalWithoutClaim();

        var result = principal.TryGetTenantId(out var tenantId);

        result.Should().BeFalse();
        tenantId.Should().Be(Guid.Empty);
    }

    [Fact]
    public void TryGetTenantId_WhenClaimMalformed_ReturnsFalseAndEmpty()
    {
        var principal = PrincipalWithClaim(TenantIdClaimType, "not-a-guid");

        var result = principal.TryGetTenantId(out var tenantId);

        result.Should().BeFalse();
        tenantId.Should().Be(Guid.Empty);
    }

    [Fact]
    public void GetTenantId_WhenClaimPresent_ReturnsGuid()
    {
        var expected = Guid.NewGuid();
        var principal = PrincipalWithClaim(TenantIdClaimType, expected.ToString());

        var tenantId = principal.GetTenantId();

        tenantId.Should().Be(expected);
    }

    [Fact]
    public void GetTenantId_WhenClaimMissing_ThrowsInvalidOperationException()
    {
        var principal = PrincipalWithoutClaim();

        var act = () => principal.GetTenantId();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*tenant_id*");
    }
}
