using MediatR;
using ProposalManagement.Application.Common.Models;

namespace ProposalManagement.Application.Audit.Queries;

public record GetAuditTrailQuery(
    string? Search,
    string? Module,
    string? Action,
    DateTime? FromDate,
    DateTime? ToDate,
    int PageIndex = 1,
    int PageSize = 20
) : IRequest<PagedResult<AuditTrailDto>>;

public record AuditTrailDto(
    long Id,
    DateTime Timestamp,
    Guid? UserId,
    string UserName,
    string UserRole,
    string IpAddress,
    string Action,
    string EntityType,
    string EntityId,
    string Description,
    string? OldValues,
    string? NewValues,
    string Module,
    string Severity
);
