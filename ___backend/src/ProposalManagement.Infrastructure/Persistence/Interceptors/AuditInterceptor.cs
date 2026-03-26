using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using ProposalManagement.Application.Common.Interfaces;
using ProposalManagement.Domain.Entities;
using ProposalManagement.Domain.Enums;

namespace ProposalManagement.Infrastructure.Persistence.Interceptors;

public class AuditInterceptor : SaveChangesInterceptor
{
    private readonly IAuditContext _auditContext;

    // Properties that must never appear in audit OldValues/NewValues
    private static readonly HashSet<string> SensitiveProperties = new(StringComparer.OrdinalIgnoreCase)
    {
        "PasswordHash", "OtpHash", "Token", "SmsGatewayApiKey"
    };

    public AuditInterceptor(IAuditContext auditContext)
    {
        _auditContext = auditContext;
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not AppDbContext context)
            return await base.SavingChangesAsync(eventData, result, cancellationToken);

        var auditEntries = new List<AuditTrail>();
        var now = DateTime.UtcNow;

        foreach (var entry in context.ChangeTracker.Entries()
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Where(e => e.Entity is not AuditTrail)) // Don't audit the audit table
        {
            var auditAction = entry.State switch
            {
                EntityState.Added => AuditAction.Create,
                EntityState.Modified => AuditAction.Update,
                EntityState.Deleted => AuditAction.Delete,
                _ => AuditAction.Update
            };

            var entityType = entry.Entity.GetType().Name;
            var entityId = GetEntityId(entry);

            string? oldValues = null;
            string? newValues = null;

            if (entry.State == EntityState.Modified)
            {
                var modifiedProps = entry.Properties
                    .Where(p => p.IsModified && !SensitiveProperties.Contains(p.Metadata.Name))
                    .ToDictionary(p => p.Metadata.Name, p => p.OriginalValue?.ToString());
                oldValues = JsonSerializer.Serialize(modifiedProps);

                var currentProps = entry.Properties
                    .Where(p => p.IsModified && !SensitiveProperties.Contains(p.Metadata.Name))
                    .ToDictionary(p => p.Metadata.Name, p => p.CurrentValue?.ToString());
                newValues = JsonSerializer.Serialize(currentProps);
            }
            else if (entry.State == EntityState.Added)
            {
                var props = entry.Properties
                    .Where(p => !SensitiveProperties.Contains(p.Metadata.Name))
                    .ToDictionary(p => p.Metadata.Name, p => p.CurrentValue?.ToString());
                newValues = JsonSerializer.Serialize(props);
            }
            else if (entry.State == EntityState.Deleted)
            {
                var props = entry.Properties
                    .Where(p => !SensitiveProperties.Contains(p.Metadata.Name))
                    .ToDictionary(p => p.Metadata.Name, p => p.OriginalValue?.ToString());
                oldValues = JsonSerializer.Serialize(props);
            }

            var module = DetermineModule(entityType);

            auditEntries.Add(new AuditTrail
            {
                Timestamp = now,
                UserId = _auditContext.UserId,
                UserName = _auditContext.UserName,
                UserRole = _auditContext.UserRole,
                IpAddress = _auditContext.IpAddress,
                UserAgent = _auditContext.UserAgent,
                Action = auditAction,
                EntityType = entityType,
                EntityId = entityId,
                Description = $"{auditAction} on {entityType} [{entityId}]",
                OldValues = oldValues,
                NewValues = newValues,
                Module = module,
                Severity = AuditSeverity.Info
            });
        }

        if (auditEntries.Count > 0)
        {
            context.AuditTrails.AddRange(auditEntries);
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static string GetEntityId(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry)
    {
        var keyProperties = entry.Properties
            .Where(p => p.Metadata.IsPrimaryKey())
            .Select(p => p.CurrentValue?.ToString() ?? "")
            .ToArray();
        return string.Join(",", keyProperties);
    }

    private static AuditModule DetermineModule(string entityType) => entityType switch
    {
        "User" or "OtpRequest" or "RefreshToken" => AuditModule.Auth,
        "Proposal" => AuditModule.Proposal,
        "ProposalStageHistory" => AuditModule.Workflow,
        "ProposalDocument" or "GeneratedDocument" => AuditModule.Document,
        "Department" or "Designation" or "FundType" or "AccountHead"
            or "Ward" or "ProcurementMethod" or "TenderPublicationPeriod"
            or "CorporationSettings" => AuditModule.Master,
        _ => AuditModule.System
    };
}
