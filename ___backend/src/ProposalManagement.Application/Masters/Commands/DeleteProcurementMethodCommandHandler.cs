using MediatR;
using ProposalManagement.Application.Common.Interfaces;
using ProposalManagement.Application.Common.Models;
using ProposalManagement.Domain.Entities;

namespace ProposalManagement.Application.Masters.Commands;

public class DeleteProcurementMethodCommandHandler : IRequestHandler<DeleteProcurementMethodCommand, Result>
{
    private readonly IRepository<ProcurementMethod> _repo;

    public DeleteProcurementMethodCommandHandler(IRepository<ProcurementMethod> repo)
    {
        _repo = repo;
    }

    public async Task<Result> Handle(DeleteProcurementMethodCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repo.GetByIdAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result.Failure("Procurement method not found", 404);

        await _repo.DeleteAsync(entity, cancellationToken);
        return Result.Success();
    }
}
