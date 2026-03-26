using MediatR;
using ProposalManagement.Application.Common.Models;

namespace ProposalManagement.Application.Workflow.Commands;

public record ApproveStageCommand(
    Guid ProposalId,
    bool TermsAccepted,
    string? Opinion_En,
    string? Opinion_Alt,
    string? Remarks_En,
    string? Remarks_Alt
) : IRequest<Result<long>>;
