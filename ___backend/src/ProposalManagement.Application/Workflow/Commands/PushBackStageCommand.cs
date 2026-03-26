using MediatR;
using ProposalManagement.Application.Common.Models;
using ProposalManagement.Domain.Enums;

namespace ProposalManagement.Application.Workflow.Commands;

public record PushBackStageCommand(
    Guid ProposalId,
    string TargetStage,
    string Reason_En,
    string? Reason_Alt,
    string? Opinion_En,
    string? Opinion_Alt,
    string? Remarks_En,
    string? Remarks_Alt
) : IRequest<Result>;
