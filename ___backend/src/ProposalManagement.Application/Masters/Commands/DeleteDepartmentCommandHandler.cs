using MediatR;
using ProposalManagement.Application.Common.Interfaces;
using ProposalManagement.Application.Common.Models;
using ProposalManagement.Domain.Entities;

namespace ProposalManagement.Application.Masters.Commands;

public class DeleteDepartmentCommandHandler : IRequestHandler<DeleteDepartmentCommand, Result>
{
    private readonly IRepository<Department> _repo;

    public DeleteDepartmentCommandHandler(IRepository<Department> repo)
    {
        _repo = repo;
    }

    public async Task<Result> Handle(DeleteDepartmentCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repo.GetByIdAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result.Failure("Department not found", 404);

        await _repo.DeleteAsync(entity, cancellationToken);
        return Result.Success();
    }
}
