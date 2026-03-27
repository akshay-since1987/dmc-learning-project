using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProposalManagement.Application.Common.Interfaces;
using ProposalManagement.Application.Notifications;
using ProposalManagement.Infrastructure.Persistence;
using ProposalManagement.Infrastructure.Persistence.Interceptors;
using ProposalManagement.Infrastructure.Services;

namespace ProposalManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<SoftDeleteInterceptor>();
        services.AddSingleton<AuditableEntityInterceptor>();

        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            options.AddInterceptors(
                sp.GetRequiredService<SoftDeleteInterceptor>(),
                sp.GetRequiredService<AuditableEntityInterceptor>());

            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName));
        });

        services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IPdfSignatureStampService, PdfSignatureStampService>();
        services.AddHttpClient<ITranslationService, GoogleTranslationService>();

        return services;
    }
}
