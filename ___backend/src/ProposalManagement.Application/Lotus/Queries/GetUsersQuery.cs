using MediatR;
using ProposalManagement.Application.Common.Models;
using ProposalManagement.Application.Lotus.DTOs;

namespace ProposalManagement.Application.Lotus.Queries;

public record GetUsersQuery(string? Search = null, string? Role = null, int PageIndex = 1, int PageSize = 20) : IRequest<PagedResult<UserListDto>>;
