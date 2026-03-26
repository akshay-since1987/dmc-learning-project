using MediatR;
using Microsoft.EntityFrameworkCore;
using ProposalManagement.Application.Common.Interfaces;
using ProposalManagement.Application.Common.Models;
using ProposalManagement.Domain.Entities;
using ProposalManagement.Domain.Enums;

namespace ProposalManagement.Application.Audit.Queries;

public class GetAuditTrailQueryHandler : IRequestHandler<GetAuditTrailQuery, PagedResult<AuditTrailDto>>
{
    private readonly IRepository<AuditTrail> _repo;
    private readonly ICurrentUser _currentUser;

    public GetAuditTrailQueryHandler(IRepository<AuditTrail> repo, ICurrentUser currentUser)
    {
        _repo = repo;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<AuditTrailDto>> Handle(GetAuditTrailQuery request, CancellationToken cancellationToken)
    {
        var query = _repo.Query().AsNoTracking();

        // Auditor can only see proposal-related modules
        if (_currentUser.Role == UserRole.Auditor)
        {
            query = query.Where(a =>
                a.Module == AuditModule.Proposal ||
                a.Module == AuditModule.Workflow ||
                a.Module == AuditModule.Document);
        }

        // Filters
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(a =>
                a.Description.ToLower().Contains(search) ||
                a.UserName.ToLower().Contains(search) ||
                a.EntityType.ToLower().Contains(search) ||
                a.EntityId.ToLower().Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(request.Module) &&
            Enum.TryParse<AuditModule>(request.Module, true, out var moduleFilter))
        {
            query = query.Where(a => a.Module == moduleFilter);
        }

        if (!string.IsNullOrWhiteSpace(request.Action) &&
            Enum.TryParse<AuditAction>(request.Action, true, out var actionFilter))
        {
            query = query.Where(a => a.Action == actionFilter);
        }

        if (request.FromDate.HasValue)
            query = query.Where(a => a.Timestamp >= request.FromDate.Value);

        if (request.ToDate.HasValue)
            query = query.Where(a => a.Timestamp <= request.ToDate.Value.AddDays(1));

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(a => a.Timestamp)
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(a => new AuditTrailDto(
                a.Id,
                a.Timestamp,
                a.UserId,
                a.UserName,
                a.UserRole,
                a.IpAddress,
                a.Action.ToString(),
                a.EntityType,
                a.EntityId,
                a.Description,
                a.OldValues,
                a.NewValues,
                a.Module.ToString(),
                a.Severity.ToString()))
            .ToListAsync(cancellationToken);

        return new PagedResult<AuditTrailDto>(items, totalCount, request.PageIndex, request.PageSize);
    }
}
