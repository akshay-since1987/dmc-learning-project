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

        // File storage — Local (dev) or Azure Blob (prod) based on config
        var storageProvider = configuration.GetValue<string>("FileStorage:Provider") ?? "Local";
        if (string.Equals(storageProvider, "AzureBlob", StringComparison.OrdinalIgnoreCase))
        {
            var connStr = configuration["AzureBlob:ConnectionString"]!;
            var container = configuration.GetValue<string>("AzureBlob:ContainerName") ?? "uploads";
            services.AddSingleton<IFileStorageService>(_ => new AzureBlobStorageService(connStr, container));
        }
        else
        {
            services.AddSingleton<IFileStorageService>(_ => new LocalFileStorageService());
        }

        // SMS service — Simulated (dev) or HTTP gateway (prod)
        var simulateSms = configuration.GetValue<bool>("Otp:SimulateOtp");
        if (simulateSms)
        {
            services.AddSingleton<IOtpSmsService, SimulatedOtpSmsService>();
        }
        else
        {
            services.AddHttpClient<IOtpSmsService, HttpSmsGatewayService>();
        }

        // DSC — Simulated for now
        services.AddSingleton<IDscService, SimulatedDscService>();

        // PDF generation
        services.AddScoped<IPdfGenerationService, QuestPdfGenerationService>();

        return services;
    }
}
