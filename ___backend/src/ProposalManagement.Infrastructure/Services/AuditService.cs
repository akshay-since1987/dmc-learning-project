using ProposalManagement.Application.Common.Interfaces;
using ProposalManagement.Domain.Entities;
using ProposalManagement.Domain.Enums;
using ProposalManagement.Infrastructure.Persistence;

namespace ProposalManagement.Infrastructure.Services;

public class AuditService : IAuditService
{
    private readonly AppDbContext _context;
    private readonly IAuditContext _auditContext;

    public AuditService(AppDbContext context, IAuditContext auditContext)
    {
        _context = context;
        _auditContext = auditContext;
    }

    public async Task LogAsync(
        AuditAction action,
        string entityType,
        string entityId,
        string description,
        AuditModule module,
        AuditSeverity severity = AuditSeverity.Info,
        string? oldValues = null,
        string? newValues = null,
        string? metadata = null,
        CancellationToken cancellationToken = default)
    {
        var audit = new AuditTrail
        {
            Timestamp = DateTime.UtcNow,
            UserId = _auditContext.UserId,
            UserName = _auditContext.UserName,
            UserRole = _auditContext.UserRole,
            IpAddress = _auditContext.IpAddress,
            UserAgent = _auditContext.UserAgent,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Description = description,
            OldValues = oldValues,
            NewValues = newValues,
            Metadata = metadata,
            Module = module,
            Severity = severity
        };

        _context.AuditTrails.Add(audit);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
