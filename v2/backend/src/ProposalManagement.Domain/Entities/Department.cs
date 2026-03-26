using ProposalManagement.Domain.Common;

namespace ProposalManagement.Domain.Entities;

public class Department : BaseAuditableEntity
{
    public Guid PalikaId { get; set; }
    public string Name_En { get; set; } = default!;
    public string? Name_Mr { get; set; }
    public string? Code { get; set; }

    // Navigation
    public Palika Palika { get; set; } = default!;
    public ICollection<DeptWorkCategory> WorkCategories { get; set; } = [];
    public ICollection<BudgetHead> BudgetHeads { get; set; } = [];
}
