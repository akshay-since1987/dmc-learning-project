using ProposalManagement.Domain.Common;

namespace ProposalManagement.Domain.Entities;

public class RequestSource : BaseAuditableEntity
{
    public Guid PalikaId { get; set; }
    public string Name_En { get; set; } = default!;
    public string? Name_Mr { get; set; }

    // Navigation
    public Palika Palika { get; set; } = default!;
}

public class SiteCondition : BaseAuditableEntity
{
    public string Name_En { get; set; } = default!;
    public string? Name_Mr { get; set; }
    public int SortOrder { get; set; }
}

public class WorkExecutionMethod : BaseAuditableEntity
{
    public Guid PalikaId { get; set; }
    public string Name_En { get; set; } = default!;
    public string? Name_Mr { get; set; }

    // Navigation
    public Palika Palika { get; set; } = default!;
}

public class FundType : BaseAuditableEntity
{
    public Guid PalikaId { get; set; }
    public string Name_En { get; set; } = default!;
    public string? Name_Mr { get; set; }

    // Navigation
    public Palika Palika { get; set; } = default!;
}
