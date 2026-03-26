using ProposalManagement.Domain.Enums;

namespace ProposalManagement.Domain.Entities;

public class ProposalStageHistory
{
    public long Id { get; set; }
    public Guid ProposalId { get; set; }
    public ProposalStage FromStage { get; set; }
    public ProposalStage ToStage { get; set; }
    public WorkflowAction Action { get; set; }
    public Guid ActionById { get; set; }
    public string ActionByName_En { get; set; } = string.Empty;
    public string ActionByName_Alt { get; set; } = string.Empty;
    public string ActionByDesignation_En { get; set; } = string.Empty;
    public string ActionByDesignation_Alt { get; set; } = string.Empty;
    public string? Reason_En { get; set; }
    public string? Reason_Alt { get; set; }
    public string? Opinion_En { get; set; }
    public string? Opinion_Alt { get; set; }
    public string? Remarks_En { get; set; }
    public string? Remarks_Alt { get; set; }
    public string? DscSignatureRef { get; set; }
    public DateTime? DscSignedAt { get; set; }
    public ProposalStage? PushedBackToStage { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation
    public Proposal Proposal { get; set; } = null!;
    public User ActionBy { get; set; } = null!;
}
