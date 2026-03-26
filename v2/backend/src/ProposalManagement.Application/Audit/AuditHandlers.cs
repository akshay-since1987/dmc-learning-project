using MediatR;
using Microsoft.EntityFrameworkCore;
using ProposalManagement.Application.Common.Interfaces;
using ProposalManagement.Application.Common.Models;
using ProposalManagement.Application.Notifications;

namespace ProposalManagement.Application.Audit;

// ── DTOs ──
public record AuditTrailDto(long Id, DateTime Timestamp, Guid? UserId, string? UserName, string? UserRole,
    string Action, string EntityType, string? EntityId, string? Description, string Module, string Severity);

// ── Query ──
public record GetAuditTrailQuery : IRequest<Result<PagedList<AuditTrailDto>>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? EntityType { get; init; }
    public string? Action { get; init; }
    public string? Module { get; init; }
    public string? UserId { get; init; }
    public DateTime? From { get; init; }
    public DateTime? To { get; init; }
    public string? Search { get; init; }
}

public class GetAuditTrailHandler(IAppDbContext db, ICurrentUser user) : IRequestHandler<GetAuditTrailQuery, Result<PagedList<AuditTrailDto>>>
{
    private static readonly HashSet<string> AllowedRoles = new() { "Lotus", "Commissioner", "Auditor" };
    private static readonly HashSet<string> AuditorModules = new() { "Proposal", "FieldVisit", "Estimate", "TS", "Prama", "Budget", "Workflow", "Document" };

    public async Task<Result<PagedList<AuditTrailDto>>> Handle(GetAuditTrailQuery request, CancellationToken ct)
    {
        if (!AllowedRoles.Contains(user.Role ?? "")) return Result<PagedList<AuditTrailDto>>.Forbidden("Access denied");

        var q = db.AuditTrails.AsQueryable();

        // Auditor can only see proposal-related modules
        if (user.Role == "Auditor")
            q = q.Where(a => AuditorModules.Contains(a.Module));

        // Palika scope (except Lotus who can see all)
        if (user.Role != "Lotus" && user.PalikaId.HasValue)
            q = q.Where(a => a.PalikaId == user.PalikaId);

        // Filters
        if (!string.IsNullOrWhiteSpace(request.EntityType)) q = q.Where(a => a.EntityType == request.EntityType);
        if (!string.IsNullOrWhiteSpace(request.Action)) q = q.Where(a => a.Action == request.Action);
        if (!string.IsNullOrWhiteSpace(request.Module)) q = q.Where(a => a.Module == request.Module);
        if (!string.IsNullOrWhiteSpace(request.UserId) && Guid.TryParse(request.UserId, out var uid))
            q = q.Where(a => a.UserId == uid);
        if (request.From.HasValue) q = q.Where(a => a.Timestamp >= request.From.Value);
        if (request.To.HasValue) q = q.Where(a => a.Timestamp <= request.To.Value.AddDays(1));
        if (!string.IsNullOrWhiteSpace(request.Search))
            q = q.Where(a => a.Description != null && a.Description.Contains(request.Search));

        var total = await q.CountAsync(ct);
        var items = await q.OrderByDescending(a => a.Timestamp)
            .Skip((request.Page - 1) * request.PageSize).Take(request.PageSize)
            .Select(a => new AuditTrailDto(a.Id, a.Timestamp, a.UserId, a.UserName, a.UserRole,
                a.Action, a.EntityType, a.EntityId, a.Description, a.Module, a.Severity))
            .ToListAsync(ct);

        return Result<PagedList<AuditTrailDto>>.Success(new(items, total, request.Page, request.PageSize));
    }
}
