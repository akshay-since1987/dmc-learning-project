using MediatR;
using ProposalManagement.Application.Common.Models;
using ProposalManagement.Application.Proposals.DTOs;

namespace ProposalManagement.Application.Proposals.Queries;

public record GetProposalByIdQuery(Guid Id) : IRequest<Result<ProposalDetailDto>>;
