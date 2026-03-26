using MediatR;
using ProposalManagement.Application.Common.Interfaces;
using ProposalManagement.Application.Common.Models;
using ProposalManagement.Domain.Entities;

namespace ProposalManagement.Application.Masters.Commands;

public class DeleteFundTypeCommandHandler : IRequestHandler<DeleteFundTypeCommand, Result>
{
    private readonly IRepository<FundType> _repo;

    public DeleteFundTypeCommandHandler(IRepository<FundType> repo)
    {
        _repo = repo;
    }

    public async Task<Result> Handle(DeleteFundTypeCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repo.GetByIdAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result.Failure("Fund type not found", 404);

        await _repo.DeleteAsync(entity, cancellationToken);
        return Result.Success();
    }
}
