using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Scalar.AspNetCore;
using Serilog;
using Sophrosync.Notes.Application.Commands.CreateNote;
using Sophrosync.Notes.Infrastructure;
using Sophrosync.SharedKernel.Abstractions;
using Sophrosync.SharedKernel.Behaviors;
using Sophrosync.SharedKernel.Security;
using Sophrosync.SharedKernel.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Service", "Notes"));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
    options.SwaggerDoc("v1", new() { Title = "Notes API", Version = "v1" }));

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
    options.AddPolicy("CanSignNotes", p =>
        p.RequireClaim("roles", "therapist", "supervisor"));

    options.AddPolicy("CanManageNotes", p =>
        p.RequireClaim("roles", "admin", "therapist", "supervisor"));

    options.AddPolicy("CanReadNotes", p =>
        p.RequireClaim("roles", "admin", "therapist", "supervisor"));
});

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(CreateNoteCommand).Assembly);
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
});

builder.Services.AddValidatorsFromAssembly(typeof(CreateNoteCommand).Assembly);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentTenant, CurrentTenantService>();
builder.Services.AddScoped<ICurrentUser, CurrentUserService>();
builder.Services.AddScoped<IClaimsTransformation, KeycloakRolesTransformation>();

builder.Services.AddNotesInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler(exceptionApp => exceptionApp.Run(async context =>
{
    var error = context.Features.Get<IExceptionHandlerFeature>()?.Error;
    if (error is ValidationException ve)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsJsonAsync(new
        {
            title = "Validation failed",
            status = 400,
            errors = ve.Errors.Select(e => e.ErrorMessage),
        });
    }
    else if (error is UnauthorizedAccessException)
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new { error = error.Message });
    }
    else if (error is InvalidOperationException)
    {
        context.Response.StatusCode = StatusCodes.Status409Conflict;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new { error = error.Message });
    }
    else if (app.Environment.IsDevelopment())
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new
        {
            title = error?.GetType().FullName ?? "UnknownError",
            detail = error?.Message,
            innerException = error?.InnerException?.Message,
            stackTrace = error?.StackTrace,
        });
    }
    else
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
    }
}));

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
