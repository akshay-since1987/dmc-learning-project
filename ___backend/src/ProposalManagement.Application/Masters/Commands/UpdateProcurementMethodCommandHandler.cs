using MediatR;
using Microsoft.EntityFrameworkCore;
using ProposalManagement.Application.Common.Interfaces;
using ProposalManagement.Application.Common.Models;
using ProposalManagement.Domain.Entities;

namespace ProposalManagement.Application.Masters.Commands;

public class UpdateProcurementMethodCommandHandler : IRequestHandler<UpdateProcurementMethodCommand, Result>
{
    private readonly IRepository<ProcurementMethod> _repo;

    public UpdateProcurementMethodCommandHandler(IRepository<ProcurementMethod> repo)
    {
        _repo = repo;
    }

    public async Task<Result> Handle(UpdateProcurementMethodCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repo.GetByIdAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result.Failure("Procurement method not found", 404);

        var duplicate = await _repo.Query()
            .AnyAsync(d => d.Name_En == request.Name_En && d.Id != request.Id, cancellationToken);
        if (duplicate)
            return Result.Failure($"Procurement method '{request.Name_En}' already exists", 409);

        entity.Name_En = request.Name_En;
        entity.Name_Alt = request.Name_Alt;
        entity.IsActive = request.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await _repo.UpdateAsync(entity, cancellationToken);
        return Result.Success();
    }
}
