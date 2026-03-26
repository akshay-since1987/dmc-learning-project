namespace ProposalManagement.Application.Workflow.DTOs;

public record StageHistoryDto(
    long Id,
    string FromStage,
    string ToStage,
    string Action,
    Guid ActionById,
    string ActionByName_En,
    string ActionByName_Alt,
    string ActionByDesignation_En,
    string ActionByDesignation_Alt,
    string? Reason_En,
    string? Reason_Alt,
    string? Opinion_En,
    string? Opinion_Alt,
    string? Remarks_En,
    string? Remarks_Alt,
    string? DscSignatureRef,
    DateTime? DscSignedAt,
    string? PushedBackToStage,
    DateTime CreatedAt);
