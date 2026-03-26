using MediatR;
using ProposalManagement.Application.Common.Interfaces;
using ProposalManagement.Application.Common.Models;
using ProposalManagement.Domain.Entities;

namespace ProposalManagement.Application.Masters.Commands;

public class DeleteAccountHeadCommandHandler : IRequestHandler<DeleteAccountHeadCommand, Result>
{
    private readonly IRepository<AccountHead> _repo;

    public DeleteAccountHeadCommandHandler(IRepository<AccountHead> repo)
    {
        _repo = repo;
    }

    public async Task<Result> Handle(DeleteAccountHeadCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repo.GetByIdAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result.Failure("Account head not found", 404);

        await _repo.DeleteAsync(entity, cancellationToken);
        return Result.Success();
    }
}
