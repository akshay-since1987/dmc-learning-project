using MediatR;
using ProposalManagement.Application.Common.Interfaces;
using ProposalManagement.Application.Common.Models;
using ProposalManagement.Domain.Entities;

namespace ProposalManagement.Application.Masters.Commands;

public class DeleteDesignationCommandHandler : IRequestHandler<DeleteDesignationCommand, Result>
{
    private readonly IRepository<Designation> _repo;

    public DeleteDesignationCommandHandler(IRepository<Designation> repo)
    {
        _repo = repo;
    }

    public async Task<Result> Handle(DeleteDesignationCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repo.GetByIdAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result.Failure("Designation not found", 404);

        await _repo.DeleteAsync(entity, cancellationToken);
        return Result.Success();
    }
}
