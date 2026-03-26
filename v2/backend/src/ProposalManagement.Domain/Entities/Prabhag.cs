using ProposalManagement.Domain.Common;

namespace ProposalManagement.Domain.Entities;

public class Prabhag : BaseAuditableEntity
{
    public Guid PalikaId { get; set; }
    public Guid ZoneId { get; set; }
    public int Number { get; set; }
    public string Name_En { get; set; } = default!;
    public string? Name_Mr { get; set; }
    public int CorporatorSeats { get; set; } = 4;
    public int? Population { get; set; }

    // Navigation
    public Palika Palika { get; set; } = default!;
    public Zone Zone { get; set; } = default!;
}
