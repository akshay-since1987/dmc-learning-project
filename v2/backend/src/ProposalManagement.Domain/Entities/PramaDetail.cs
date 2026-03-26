using ProposalManagement.Domain.Common;

namespace ProposalManagement.Domain.Entities;

public class PramaDetail : BaseEntity
{
    public Guid ProposalId { get; set; }

    public Guid? FundTypeId { get; set; }
    public Guid? BudgetHeadId { get; set; }
    public string? FundApprovalYear { get; set; }
    public string? DeptUserName_En { get; set; }
    public string? DeptUserName_Mr { get; set; }
    public string? References_En { get; set; }
    public string? References_Mr { get; set; }
    public string? AdditionalDetails_En { get; set; }
    public string? AdditionalDetails_Mr { get; set; }

    // Navigation
    public Proposal Proposal { get; set; } = default!;
    public FundType? FundType { get; set; }
    public BudgetHead? BudgetHead { get; set; }
}
