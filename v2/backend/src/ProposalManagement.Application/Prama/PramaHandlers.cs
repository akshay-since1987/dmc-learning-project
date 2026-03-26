using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProposalManagement.Application.Common.Interfaces;
using ProposalManagement.Application.Common.Models;
using ProposalManagement.Domain.Entities;
using ProposalManagement.Domain.Enums;

namespace ProposalManagement.Application.Prama;

// ── DTOs ──
public record PramaDetailDto
{
    public Guid Id { get; init; }
    public Guid ProposalId { get; init; }
    public Guid? FundTypeId { get; init; }
    public string? FundTypeName { get; init; }
    public Guid? BudgetHeadId { get; init; }
    public string? BudgetHeadName { get; init; }
    public string? FundApprovalYear { get; init; }
    public string? DeptUserName_En { get; init; }
    public string? DeptUserName_Mr { get; init; }
    public string? References_En { get; init; }
    public string? References_Mr { get; init; }
    public string? AdditionalDetails_En { get; init; }
    public string? AdditionalDetails_Mr { get; init; }
    public DateTime CreatedAt { get; init; }
}

// ── Query ──
public record GetPramaDetailQuery(Guid ProposalId) : IRequest<Result<PramaDetailDto?>>;

public class GetPramaDetailHandler(IAppDbContext db) : IRequestHandler<GetPramaDetailQuery, Result<PramaDetailDto?>>
{
    public async Task<Result<PramaDetailDto?>> Handle(GetPramaDetailQuery request, CancellationToken ct)
    {
        var p = await db.PramaDetails
            .Where(x => x.ProposalId == request.ProposalId && !x.IsDeleted)
            .Include(x => x.FundType).Include(x => x.BudgetHead)
            .FirstOrDefaultAsync(ct);

        if (p is null) return Result<PramaDetailDto?>.Success(null);
        return Result<PramaDetailDto?>.Success(new PramaDetailDto
        {
            Id = p.Id, ProposalId = p.ProposalId, FundTypeId = p.FundTypeId,
            FundTypeName = p.FundType?.Name_En, BudgetHeadId = p.BudgetHeadId,
            BudgetHeadName = p.BudgetHead?.Name_En, FundApprovalYear = p.FundApprovalYear,
            DeptUserName_En = p.DeptUserName_En, DeptUserName_Mr = p.DeptUserName_Mr,
            References_En = p.References_En, References_Mr = p.References_Mr,
            AdditionalDetails_En = p.AdditionalDetails_En, AdditionalDetails_Mr = p.AdditionalDetails_Mr,
            CreatedAt = p.CreatedAt
        });
    }
}

// ── Command ──
public record SavePramaDetailCommand : IRequest<Result<Guid>>
{
    public Guid ProposalId { get; init; }
    public Guid? FundTypeId { get; init; }
    public Guid? BudgetHeadId { get; init; }
    public string? FundApprovalYear { get; init; }
    public string? DeptUserName_En { get; init; }
    public string? DeptUserName_Mr { get; init; }
    public string? References_En { get; init; }
    public string? References_Mr { get; init; }
    public string? AdditionalDetails_En { get; init; }
    public string? AdditionalDetails_Mr { get; init; }
}

public class SavePramaDetailHandler(IAppDbContext db, ICurrentUser user, ILogger<SavePramaDetailHandler> logger) 
    : IRequestHandler<SavePramaDetailCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(SavePramaDetailCommand request, CancellationToken ct)
    {
        var existing = await db.PramaDetails.FirstOrDefaultAsync(x => x.ProposalId == request.ProposalId && !x.IsDeleted, ct);

        if (existing is not null)
        {
            existing.FundTypeId = request.FundTypeId;
            existing.BudgetHeadId = request.BudgetHeadId;
            existing.FundApprovalYear = request.FundApprovalYear;
            existing.DeptUserName_En = request.DeptUserName_En;
            existing.DeptUserName_Mr = request.DeptUserName_Mr;
            existing.References_En = request.References_En;
            existing.References_Mr = request.References_Mr;
            existing.AdditionalDetails_En = request.AdditionalDetails_En;
            existing.AdditionalDetails_Mr = request.AdditionalDetails_Mr;
            await db.SaveChangesAsync(ct);
            return Result<Guid>.Success(existing.Id);
        }

        var pd = new PramaDetail
        {
            Id = Guid.NewGuid(), ProposalId = request.ProposalId,
            FundTypeId = request.FundTypeId, BudgetHeadId = request.BudgetHeadId,
            FundApprovalYear = request.FundApprovalYear,
            DeptUserName_En = request.DeptUserName_En, DeptUserName_Mr = request.DeptUserName_Mr,
            References_En = request.References_En, References_Mr = request.References_Mr,
            AdditionalDetails_En = request.AdditionalDetails_En, AdditionalDetails_Mr = request.AdditionalDetails_Mr,
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        };
        db.PramaDetails.Add(pd);

        var proposal = await db.Proposals.FindAsync(new object[] { request.ProposalId }, ct);
        if (proposal is not null && proposal.CompletedTab < 5) proposal.CompletedTab = 5;

        await db.SaveChangesAsync(ct);
        logger.LogInformation("PRAMA detail saved for Proposal {ProposalId}", request.ProposalId);
        return Result<Guid>.Success(pd.Id);
    }
}
