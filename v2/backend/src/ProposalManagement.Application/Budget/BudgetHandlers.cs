using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProposalManagement.Application.Common.Interfaces;
using ProposalManagement.Application.Common.Models;
using ProposalManagement.Domain.Entities;
using ProposalManagement.Domain.Enums;

namespace ProposalManagement.Application.Budget;

// ── DTOs ──
public record BudgetDetailDto
{
    public Guid Id { get; init; }
    public Guid ProposalId { get; init; }
    public Guid? WorkExecutionMethodId { get; init; }
    public string? WorkExecutionMethodName { get; init; }
    public string? WorkExecutionMethodName_Mr { get; init; }
    public int? WorkDurationDays { get; init; }
    public bool TenderVerificationDone { get; init; }
    public Guid? BudgetHeadId { get; init; }
    public string? BudgetHeadName { get; init; }
    public string? BudgetHeadName_Mr { get; init; }
    public decimal? AllocatedFund { get; init; }
    public decimal? CurrentAvailableFund { get; init; }
    public decimal? OldExpenditure { get; init; }
    public decimal? EstimatedCost { get; init; }
    public decimal? BalanceAmount { get; init; }
    public string? AccountSerialNo { get; init; }
    public string? ComplianceNotes_En { get; init; }
    public string? ComplianceNotes_Mr { get; init; }
    public string? DeterminedApprovalSlab { get; init; }
    public string? FinalAuthorityRole { get; init; }
    public DateTime CreatedAt { get; init; }
}

// ── Query ──
public record GetBudgetDetailQuery(Guid ProposalId) : IRequest<Result<BudgetDetailDto?>>;

public class GetBudgetDetailHandler(IAppDbContext db) : IRequestHandler<GetBudgetDetailQuery, Result<BudgetDetailDto?>>
{
    public async Task<Result<BudgetDetailDto?>> Handle(GetBudgetDetailQuery request, CancellationToken ct)
    {
        var b = await db.BudgetDetails
            .Where(x => x.ProposalId == request.ProposalId && !x.IsDeleted)
            .Include(x => x.WorkExecutionMethod).Include(x => x.BudgetHead)
            .FirstOrDefaultAsync(ct);

        if (b is null) return Result<BudgetDetailDto?>.Success(null);
        return Result<BudgetDetailDto?>.Success(new BudgetDetailDto
        {
            Id = b.Id, ProposalId = b.ProposalId,
            WorkExecutionMethodId = b.WorkExecutionMethodId, WorkExecutionMethodName = b.WorkExecutionMethod?.Name_En,
            WorkExecutionMethodName_Mr = b.WorkExecutionMethod?.Name_Mr,
            WorkDurationDays = b.WorkDurationDays, TenderVerificationDone = b.TenderVerificationDone,
            BudgetHeadId = b.BudgetHeadId, BudgetHeadName = b.BudgetHead?.Name_En,
            BudgetHeadName_Mr = b.BudgetHead?.Name_Mr,
            AllocatedFund = b.AllocatedFund, CurrentAvailableFund = b.CurrentAvailableFund,
            OldExpenditure = b.OldExpenditure, EstimatedCost = b.EstimatedCost,
            BalanceAmount = b.BalanceAmount, AccountSerialNo = b.AccountSerialNo,
            ComplianceNotes_En = b.ComplianceNotes_En, ComplianceNotes_Mr = b.ComplianceNotes_Mr,
            DeterminedApprovalSlab = b.DeterminedApprovalSlab, FinalAuthorityRole = b.FinalAuthorityRole,
            CreatedAt = b.CreatedAt
        });
    }
}

// ── Command ──
public record SaveBudgetDetailCommand : IRequest<Result<Guid>>
{
    public Guid ProposalId { get; init; }
    public Guid? WorkExecutionMethodId { get; init; }
    public int? WorkDurationDays { get; init; }
    public bool TenderVerificationDone { get; init; }
    public Guid? BudgetHeadId { get; init; }
    public decimal? AllocatedFund { get; init; }
    public decimal? CurrentAvailableFund { get; init; }
    public decimal? OldExpenditure { get; init; }
    public decimal? EstimatedCost { get; init; }
    public string? AccountSerialNo { get; init; }
    public string? ComplianceNotes_En { get; init; }
    public string? ComplianceNotes_Mr { get; init; }
}

public class SaveBudgetDetailHandler(IAppDbContext db, ICurrentUser user, ILogger<SaveBudgetDetailHandler> logger) 
    : IRequestHandler<SaveBudgetDetailCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(SaveBudgetDetailCommand request, CancellationToken ct)
    {
        // Auto-compute
        var balance = (request.AllocatedFund ?? 0) - (request.OldExpenditure ?? 0) - (request.EstimatedCost ?? 0);
        var (slab, authority) = DetermineApprovalAuthority(request.EstimatedCost ?? 0);

        var existing = await db.BudgetDetails.FirstOrDefaultAsync(x => x.ProposalId == request.ProposalId && !x.IsDeleted, ct);

        if (existing is not null)
        {
            existing.WorkExecutionMethodId = request.WorkExecutionMethodId;
            existing.WorkDurationDays = request.WorkDurationDays;
            existing.TenderVerificationDone = request.TenderVerificationDone;
            existing.BudgetHeadId = request.BudgetHeadId;
            existing.AllocatedFund = request.AllocatedFund;
            existing.CurrentAvailableFund = request.CurrentAvailableFund;
            existing.OldExpenditure = request.OldExpenditure;
            existing.EstimatedCost = request.EstimatedCost;
            existing.BalanceAmount = balance;
            existing.AccountSerialNo = request.AccountSerialNo;
            existing.ComplianceNotes_En = request.ComplianceNotes_En;
            existing.ComplianceNotes_Mr = request.ComplianceNotes_Mr;
            existing.DeterminedApprovalSlab = slab;
            existing.FinalAuthorityRole = authority;
            await db.SaveChangesAsync(ct);
            return Result<Guid>.Success(existing.Id);
        }

        var bd = new BudgetDetail
        {
            Id = Guid.NewGuid(), ProposalId = request.ProposalId,
            WorkExecutionMethodId = request.WorkExecutionMethodId, WorkDurationDays = request.WorkDurationDays,
            TenderVerificationDone = request.TenderVerificationDone, BudgetHeadId = request.BudgetHeadId,
            AllocatedFund = request.AllocatedFund, CurrentAvailableFund = request.CurrentAvailableFund,
            OldExpenditure = request.OldExpenditure, EstimatedCost = request.EstimatedCost,
            BalanceAmount = balance, AccountSerialNo = request.AccountSerialNo,
            ComplianceNotes_En = request.ComplianceNotes_En, ComplianceNotes_Mr = request.ComplianceNotes_Mr,
            DeterminedApprovalSlab = slab, FinalAuthorityRole = authority,
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        };
        db.BudgetDetails.Add(bd);

        var proposal = await db.Proposals.FindAsync(new object[] { request.ProposalId }, ct);
        if (proposal is not null && proposal.CompletedTab < 6) proposal.CompletedTab = 6;

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Budget detail saved for Proposal {ProposalId}", request.ProposalId);
        return Result<Guid>.Success(bd.Id);
    }

    private static (string slab, string authority) DetermineApprovalAuthority(decimal estimatedCost)
    {
        if (estimatedCost <= 300000) return (nameof(ApprovalSlab.Slab0to3L), nameof(UserRole.DyCommissioner));
        if (estimatedCost <= 2400000) return (nameof(ApprovalSlab.Slab3to24L), nameof(UserRole.Commissioner));
        if (estimatedCost <= 2500000) return (nameof(ApprovalSlab.Slab24to25L), nameof(UserRole.StandingCommittee));
        return (nameof(ApprovalSlab.Slab25LPlus), nameof(UserRole.Collector));
    }
}
