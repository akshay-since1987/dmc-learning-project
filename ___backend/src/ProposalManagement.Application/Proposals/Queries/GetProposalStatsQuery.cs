using MediatR;

namespace ProposalManagement.Application.Proposals.Queries;

public record GetProposalStatsQuery : IRequest<ProposalStatsDto>;

public record ProposalStatsDto(
    int Total,
    int Pending,
    int Approved,
    int PushedBack,
    int Draft);
