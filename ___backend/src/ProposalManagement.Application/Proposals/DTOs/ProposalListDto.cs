namespace ProposalManagement.Application.Proposals.DTOs;

public record ProposalListDto(
    Guid Id,
    string ProposalNumber,
    DateOnly Date,
    string Subject_En,
    string Subject_Alt,
    string DepartmentName_En,
    string CurrentStage,
    decimal EstimatedCost,
    string SubmittedByName_En,
    int PushBackCount,
    DateTime CreatedAt);
