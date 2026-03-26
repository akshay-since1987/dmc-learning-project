using ProposalManagement.Domain.Common;

namespace ProposalManagement.Domain.Entities;

public class Zone : BaseAuditableEntity
{
    public Guid PalikaId { get; set; }
    public string Name_En { get; set; } = default!;
    public string? Name_Mr { get; set; }
    public string? Code { get; set; }
    public string? OfficeName_En { get; set; }
    public string? OfficeName_Mr { get; set; }

    // Navigation
    public Palika Palika { get; set; } = default!;
    public ICollection<Prabhag> Prabhags { get; set; } = [];
}
