using MediatR;
using ProposalManagement.Application.Common.Models;

namespace ProposalManagement.Application.Proposals.Commands;

public record DeleteProposalCommand(Guid Id) : IRequest<Result>;
