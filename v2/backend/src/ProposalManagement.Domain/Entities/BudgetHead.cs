using ProposalManagement.Domain.Common;

namespace ProposalManagement.Domain.Entities;

public class BudgetHead : BaseAuditableEntity
{
    public Guid PalikaId { get; set; }
    public Guid DepartmentId { get; set; }
    public Guid FundTypeId { get; set; }
    public string Code { get; set; } = default!;
    public string Name_En { get; set; } = default!;
    public string? Name_Mr { get; set; }
    public string FinancialYear { get; set; } = default!;
    public decimal AllocatedAmount { get; set; }
    public decimal CurrentAvailable { get; set; }

    // Navigation
    public Palika Palika { get; set; } = default!;
    public Department Department { get; set; } = default!;
    public FundType FundType { get; set; } = default!;
}
