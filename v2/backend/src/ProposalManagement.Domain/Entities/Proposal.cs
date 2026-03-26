using ProposalManagement.Domain.Common;

namespace ProposalManagement.Domain.Entities;

public class Proposal : BaseEntity
{
    public string ProposalNumber { get; set; } = default!;
    public Guid PalikaId { get; set; }
    public DateTime ProposalDate { get; set; }
    public Guid DepartmentId { get; set; }
    public Guid DeptWorkCategoryId { get; set; }
    public Guid CreatedById { get; set; }

    // Location
    public Guid ZoneId { get; set; }
    public Guid PrabhagId { get; set; }
    public string? Area { get; set; }
    public string? LocationAddress_En { get; set; }
    public string? LocationAddress_Mr { get; set; }
    public string? LocationMapPath { get; set; }

    // Work Details
    public string WorkTitle_En { get; set; } = default!;
    public string? WorkTitle_Mr { get; set; }
    public string WorkDescription_En { get; set; } = default!;
    public string? WorkDescription_Mr { get; set; }

    // Request Source
    public Guid? RequestSourceId { get; set; }
    public string? RequestorName { get; set; }
    public string? RequestorMobile { get; set; }
    public string? RequestorAddress { get; set; }
    public string? RequestorDesignation { get; set; }
    public string? RequestorOrganisation { get; set; }

    public string Priority { get; set; } = Enums.Priority.Medium.ToString();

    // Workflow State
    public string CurrentStage { get; set; } = Enums.ProposalStage.Draft.ToString();
    public Guid? CurrentOwnerId { get; set; }
    public int PushBackCount { get; set; }
    public DateTime? ParkedAt { get; set; }
    public string? ParkedAtStage { get; set; }

    // Completion Tracking
    public int CompletedTab { get; set; } = 1;

    // Navigation
    public Palika Palika { get; set; } = default!;
    public Department Department { get; set; } = default!;
    public DeptWorkCategory DeptWorkCategory { get; set; } = default!;
    public User CreatedBy { get; set; } = default!;
    public Zone Zone { get; set; } = default!;
    public Prabhag Prabhag { get; set; } = default!;
    public RequestSource? RequestSource { get; set; }
    public User? CurrentOwner { get; set; }

    public ICollection<ProposalDocument> Documents { get; set; } = [];
    public ICollection<FieldVisit> FieldVisits { get; set; } = [];
    public ICollection<ProposalApproval> Approvals { get; set; } = [];
    public ICollection<GeneratedPdf> GeneratedPdfs { get; set; } = [];
    public ICollection<Notification> Notifications { get; set; } = [];
    public Estimate? Estimate { get; set; }
    public TechnicalSanction? TechnicalSanction { get; set; }
    public PramaDetail? PramaDetail { get; set; }
    public BudgetDetail? BudgetDetail { get; set; }
}
