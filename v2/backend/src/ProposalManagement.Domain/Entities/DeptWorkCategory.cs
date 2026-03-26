using ProposalManagement.Domain.Common;

namespace ProposalManagement.Domain.Entities;

public class DeptWorkCategory : BaseAuditableEntity
{
    public Guid? DepartmentId { get; set; }
    public string Name_En { get; set; } = default!;
    public string? Name_Mr { get; set; }

    // Navigation
    public Department? Department { get; set; }
}
