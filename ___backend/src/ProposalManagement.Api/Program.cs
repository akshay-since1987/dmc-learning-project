using ProposalManagement.Application;
using ProposalManagement.Application.Common.Interfaces;
using ProposalManagement.Infrastructure;
using ProposalManagement.Infrastructure.Services;
using ProposalManagement.Api.Middleware;
using ProposalManagement.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUserService>();
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();

// Initialize DB (migrate + seed)
using (var scope = app.Services.CreateScope())
{
    var initializer = new DbInitializationService(
        scope.ServiceProvider.GetRequiredService<ProposalManagement.Infrastructure.Persistence.AppDbContext>(),
        scope.ServiceProvider.GetRequiredService<ILogger<DbInitializationService>>());
    await initializer.InitializeAsync();
}

// Middleware pipeline
app.UseResponseCompression();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<AuditContextMiddleware>();

// In development, serve JS/CSS with no-cache headers so browser always fetches fresh files
if (app.Environment.IsDevelopment())
{
    app.UseStaticFiles(new StaticFileOptions
    {
        OnPrepareResponse = ctx =>
        {
            var ext = System.IO.Path.GetExtension(ctx.File.Name).ToLowerInvariant();
            if (ext is ".js" or ".css")
            {
                ctx.Context.Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate";
                ctx.Context.Response.Headers["Pragma"] = "no-cache";
            }
        }
    });
}
else
{
    app.UseStaticFiles();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Fallback to index.html for SPA-style client routing
app.MapFallbackToFile("index.html");

app.Run();
