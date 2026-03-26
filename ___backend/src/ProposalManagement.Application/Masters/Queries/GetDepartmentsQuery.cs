using MediatR;
using ProposalManagement.Application.Common.Models;
using ProposalManagement.Application.Masters.DTOs;

namespace ProposalManagement.Application.Masters.Queries;

public record GetDepartmentsQuery(string? Search = null, int PageIndex = 1, int PageSize = 20) : IRequest<PagedResult<MasterListItemDto>>;
