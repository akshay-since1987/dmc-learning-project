namespace ProposalManagement.Domain.Entities;

public class ProposalApproval
{
    public long Id { get; set; }
    public Guid ProposalId { get; set; }
    public string StageRole { get; set; } = default!;
    public string Action { get; set; } = default!;

    // Approver info
    public Guid ActorId { get; set; }
    public string? ActorName_En { get; set; }
    public string? ActorName_Mr { get; set; }
    public string? ActorDesignation_En { get; set; }
    public string? ActorDesignation_Mr { get; set; }

    // Disclaimer
    public string DisclaimerText { get; set; } = default!;
    public bool DisclaimerAccepted { get; set; }

    // Opinion & Signature
    public string? Opinion_En { get; set; }
    public string? Opinion_Mr { get; set; }
    public string? SignaturePath { get; set; }

    // Push-back specific
    public string? PushBackNote_En { get; set; }
    public string? PushBackNote_Mr { get; set; }

    // PDF
    public string? ConsolidatedPdfPath { get; set; }

    public DateTime CreatedAt { get; set; }

    // Navigation
    public Proposal Proposal { get; set; } = default!;
    public User Actor { get; set; } = default!;
}
