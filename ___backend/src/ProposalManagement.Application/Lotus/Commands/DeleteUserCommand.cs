using MediatR;
using Microsoft.EntityFrameworkCore;
using ProposalManagement.Application.Common.Interfaces;
using ProposalManagement.Application.Common.Models;
using ProposalManagement.Domain.Entities;

namespace ProposalManagement.Application.Lotus.Commands;

public record DeleteUserCommand(Guid Id) : IRequest<Result>;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, Result>
{
    private readonly IRepository<User> _repo;

    public DeleteUserCommandHandler(IRepository<User> repo)
    {
        _repo = repo;
    }

    public async Task<Result> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _repo.QueryIgnoreFilters()
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);
        if (user is null)
            return Result.Failure("User not found", 404);

        await _repo.DeleteAsync(user, cancellationToken);
        return Result.Success();
    }
}
