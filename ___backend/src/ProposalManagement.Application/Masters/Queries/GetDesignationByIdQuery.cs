using MediatR;
using ProposalManagement.Application.Common.Models;
using ProposalManagement.Application.Masters.DTOs;

namespace ProposalManagement.Application.Masters.Queries;

public record GetDesignationByIdQuery(Guid Id) : IRequest<Result<MasterDetailDto>>;
