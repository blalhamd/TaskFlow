using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Text.Json.Serialization;
using TaskFlow.API.Extensions;
using TaskFlow.API.Filters;
using TaskFlow.API.Middlewares;
using TaskFlow.DependencyInjection;
using TaskFlow.Domain.Entities.Identity;
using TaskFlow.Infrastructure.Data.context;
using TaskFlow.Infrastructure.Data.SeedData;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers(options =>
{
    options.Filters.Add<RequestTimeCalculationFilter>();
}).AddJsonOptions(options =>
{
    // Serialize enums as strings instead of numbers
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Configure Serilog with settings from appsettings.json
builder.Host.EnableSerilog();

builder.Services.RegisterConfiguration(builder.Configuration);
builder.Services.RegisterApiVersioningConfig();
builder.Services.RegisterOpenAPI();
builder.Services.RegisterOpenTelemetryConfig();
builder.Services.AddHttpContextAccessor();
builder.Services.OptionsPatternConfig(builder.Configuration);
builder.Services.RegisterJwtAuthenticationConfig(builder.Configuration);
builder.Services.EnableCors(builder.Configuration);

builder.Services.AddAuthorization();

var app = builder.Build();

// Add Serilog's request logging middleware for HTTP requests
app.UseSerilogRequestLogging();


using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();

        // Apply pending migrations
        await context.Database.MigrateAsync();

        // Run seeding
        await Seed.InitializeDataAsync(context, userManager, roleManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred during database seeding.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); 
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Project API v1");
        options.SwaggerEndpoint("/openapi/v2.json", "Project API v2");

        options.EnableDeepLinking();
        options.EnableFilter();
        options.DisplayRequestDuration();
    });
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors(); // you don't need to give name of policy because you specified defaultPolicy in CorsPolicy extension method
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.UseMiddleware<ErrorHandlingMiddleware>();

app.Run();
