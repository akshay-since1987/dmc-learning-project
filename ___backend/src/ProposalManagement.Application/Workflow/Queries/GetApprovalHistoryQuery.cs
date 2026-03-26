using MediatR;
using ProposalManagement.Application.Workflow.DTOs;

namespace ProposalManagement.Application.Workflow.Queries;

public record GetApprovalHistoryQuery(Guid ProposalId) : IRequest<IReadOnlyList<StageHistoryDto>>;
