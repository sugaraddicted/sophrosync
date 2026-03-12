using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Sophrosync.Gateway.Data;
using Sophrosync.Gateway.Middleware;
using System.Threading.RateLimiting;

// Spec ref: Architecture Spec Section 4.6 (YARP Gateway), Section 2.3 (.NET Integration),
//           Section 3.4 (Security Patterns), Section 5 (SharedKernel)

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Service", "Gateway"));

// Spec Section 2.3: JWT resource server — Keycloak RS256, JWKS auto-discovery
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Keycloak:Authority"];
        options.Audience = builder.Configuration["Keycloak:Audience"];
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
    });

// Spec Section 3.4: Default policy requires authenticated user — all YARP routes use this policy
builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// Spec Section 6: sophrosync_tenants database — tenant_profiles table provisioned by Gateway
builder.Services.AddDbContext<TenantDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("TenantsDb")));

builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            }));
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

// Spec Section 4.5 / OWASP A01: Block /internal/* paths from public access
// AuditService write endpoint and other internal routes must not be reachable externally.
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/internal"))
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        return;
    }
    await next(context);
});

// Inject X-Correlation-Id on all forwarded requests (idempotent — preserve existing header)
app.Use(async (context, next) =>
{
    if (!context.Request.Headers.ContainsKey("X-Correlation-Id"))
        context.Request.Headers["X-Correlation-Id"] = Guid.NewGuid().ToString();
    await next(context);
});

// Spec Section 4.6: Tenant provisioning — insert TenantProfile on first JWT arrival
app.UseMiddleware<TenantProvisioningMiddleware>();

app.MapReverseProxy();

app.Run();
