using ProposalManagement.Domain.Common;

namespace ProposalManagement.Domain.Entities;

public class Designation : BaseAuditableEntity
{
    public Guid PalikaId { get; set; }
    public string Name_En { get; set; } = default!;
    public string? Name_Mr { get; set; }

    // Navigation
    public Palika Palika { get; set; } = default!;
}
