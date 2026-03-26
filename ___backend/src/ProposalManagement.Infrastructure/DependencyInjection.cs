using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ProposalManagement.Application.Common.Interfaces;
using ProposalManagement.Infrastructure.Persistence;
using ProposalManagement.Infrastructure.Persistence.Interceptors;
using ProposalManagement.Infrastructure.Persistence.Repositories;
using ProposalManagement.Infrastructure.Services;
using QuestPDF.Infrastructure;

namespace ProposalManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Interceptors
        services.AddScoped<AuditInterceptor>();
        services.AddScoped<SoftDeleteInterceptor>();

        // DbContext
        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            var auditInterceptor = sp.GetRequiredService<AuditInterceptor>();
            var softDeleteInterceptor = sp.GetRequiredService<SoftDeleteInterceptor>();

            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName));

            options.AddInterceptors(softDeleteInterceptor, auditInterceptor);
        });

        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

        // Repository
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        // Audit
        services.AddScoped<IAuditContext, AuditContextService>();
        services.AddScoped<IAuditService, AuditService>();

        // Auth services
        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddHttpClient<IOtpService, OtpService>();

        // File storage
        var storagePath = configuration["FileStorage:BasePath"]
            ?? Path.Combine(Directory.GetCurrentDirectory(), "uploads");
        services.AddSingleton<IFileStorage>(new LocalFileStorageService(storagePath));

        // Translation service
        services.AddHttpClient<ITranslationService, GoogleTranslationService>();

        // PDF generation (QuestPDF community license)
        QuestPDF.Settings.License = LicenseType.Community;
        services.AddScoped<IPdfGenerationService, ProposalPdfService>();
        services.AddScoped<IPdfSignatureStampService, PdfSignatureStampService>();

        // JWT Authentication
        var jwtKey = configuration["Jwt:Key"]!;
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                ValidateIssuer = true,
                ValidIssuer = configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = configuration["Jwt:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });

        services.AddAuthorization();

        return services;
    }
}
