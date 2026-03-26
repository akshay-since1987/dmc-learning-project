using MediatR;
using ProposalManagement.Application.Common.Interfaces;
using ProposalManagement.Application.Common.Models;
using ProposalManagement.Domain.Entities;

namespace ProposalManagement.Application.Masters.Commands;

public class DeleteWardCommandHandler : IRequestHandler<DeleteWardCommand, Result>
{
    private readonly IRepository<Ward> _repo;

    public DeleteWardCommandHandler(IRepository<Ward> repo)
    {
        _repo = repo;
    }

    public async Task<Result> Handle(DeleteWardCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repo.GetByIdAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result.Failure("Ward not found", 404);

        await _repo.DeleteAsync(entity, cancellationToken);
        return Result.Success();
    }
}
