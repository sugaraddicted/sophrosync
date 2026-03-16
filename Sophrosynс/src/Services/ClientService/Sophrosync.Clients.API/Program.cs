using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Scalar.AspNetCore;
using Serilog;
using Sophrosync.Clients.Application.Commands.CreateClient;
using Sophrosync.Clients.Infrastructure;
using Sophrosync.SharedKernel.Abstractions;
using Sophrosync.SharedKernel.Behaviors;
using Sophrosync.SharedKernel.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Service", "Clients"));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
    options.SwaggerDoc("v1", new() { Title = "Clients API", Version = "v1" }));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Keycloak:Authority"];
        options.Audience = builder.Configuration["Keycloak:Audience"];
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
    });

builder.Services.AddAuthorization(options =>
{
    // Keycloak emits role assignments in the "roles" claim (not the standard ClaimTypes.Role).
    // KeycloakClaimsExtensions.GetRoles() reads both ClaimTypes.Role and "roles" — these
    // policies mirror that by targeting the "roles" claim directly.
    options.AddPolicy("CanManageClients", p =>
        p.RequireClaim("roles", "admin", "therapist", "supervisor"));

    options.AddPolicy("CanReadClients", p =>
        p.RequireClaim("roles", "admin", "therapist", "supervisor", "receptionist"));
});

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(CreateClientCommand).Assembly);
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ExceptionBehavior<,>));
});

builder.Services.AddValidatorsFromAssembly(typeof(CreateClientCommand).Assembly);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentTenant, CurrentTenantService>();
builder.Services.AddScoped<ICurrentUser, CurrentUserService>();

builder.Services.AddClientsInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.MapScalarApiReference(options =>
        options.WithOpenApiRoutePattern("/swagger/v1/swagger.json"));
}

app.Run();
