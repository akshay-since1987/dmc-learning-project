using ProposalManagement.Domain.Enums;

namespace ProposalManagement.Application.Common.Interfaces;

public interface IAuditService
{
    Task LogAsync(
        AuditAction action,
        string entityType,
        string entityId,
        string description,
        AuditModule module,
        AuditSeverity severity = AuditSeverity.Info,
        string? oldValues = null,
        string? newValues = null,
        string? metadata = null,
        CancellationToken cancellationToken = default);
}
