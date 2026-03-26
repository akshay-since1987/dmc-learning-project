using ProposalManagement.Domain.Common;

namespace ProposalManagement.Domain.Entities;

public class BudgetDetail : BaseEntity
{
    public Guid ProposalId { get; set; }

    public Guid? WorkExecutionMethodId { get; set; }
    public int? WorkDurationDays { get; set; }
    public bool TenderVerificationDone { get; set; }

    public Guid? BudgetHeadId { get; set; }
    public decimal? AllocatedFund { get; set; }
    public decimal? CurrentAvailableFund { get; set; }
    public decimal? OldExpenditure { get; set; }
    public decimal? EstimatedCost { get; set; }
    public decimal? BalanceAmount { get; set; }

    public string? AccountSerialNo { get; set; }

    // Compliance
    public string? ComplianceNotes_En { get; set; }
    public string? ComplianceNotes_Mr { get; set; }

    // Auto-determined approval authority
    public string? DeterminedApprovalSlab { get; set; }
    public string? FinalAuthorityRole { get; set; }

    // Navigation
    public Proposal Proposal { get; set; } = default!;
    public WorkExecutionMethod? WorkExecutionMethod { get; set; }
    public BudgetHead? BudgetHead { get; set; }
}
