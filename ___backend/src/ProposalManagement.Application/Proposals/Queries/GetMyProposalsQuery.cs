using MediatR;
using ProposalManagement.Application.Common.Models;
using ProposalManagement.Application.Proposals.DTOs;

namespace ProposalManagement.Application.Proposals.Queries;

public record GetMyProposalsQuery(string? Search, int PageIndex = 1, int PageSize = 20) : IRequest<PagedResult<ProposalListDto>>;
