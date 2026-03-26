using MediatR;
using Microsoft.EntityFrameworkCore;
using ProposalManagement.Application.Common.Interfaces;
using ProposalManagement.Application.Common.Models;
using ProposalManagement.Domain.Entities;

namespace ProposalManagement.Application.Masters.Commands;

public class UpdateDesignationCommandHandler : IRequestHandler<UpdateDesignationCommand, Result>
{
    private readonly IRepository<Designation> _repo;

    public UpdateDesignationCommandHandler(IRepository<Designation> repo)
    {
        _repo = repo;
    }

    public async Task<Result> Handle(UpdateDesignationCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repo.GetByIdAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result.Failure("Designation not found", 404);

        var duplicate = await _repo.Query()
            .AnyAsync(d => d.Name_En == request.Name_En && d.Id != request.Id, cancellationToken);
        if (duplicate)
            return Result.Failure($"Designation '{request.Name_En}' already exists", 409);

        entity.Name_En = request.Name_En;
        entity.Name_Alt = request.Name_Alt;
        entity.IsActive = request.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await _repo.UpdateAsync(entity, cancellationToken);
        return Result.Success();
    }
}
