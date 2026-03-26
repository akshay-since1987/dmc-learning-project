using MediatR;
using Microsoft.EntityFrameworkCore;
using ProposalManagement.Application.Common.Interfaces;
using ProposalManagement.Application.Common.Models;
using ProposalManagement.Domain.Entities;

namespace ProposalManagement.Application.Masters.Commands;

public class UpdateWardCommandHandler : IRequestHandler<UpdateWardCommand, Result>
{
    private readonly IRepository<Ward> _repo;

    public UpdateWardCommandHandler(IRepository<Ward> repo)
    {
        _repo = repo;
    }

    public async Task<Result> Handle(UpdateWardCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repo.GetByIdAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result.Failure("Ward not found", 404);

        var duplicate = await _repo.Query()
            .AnyAsync(d => d.Number == request.Number && d.Id != request.Id, cancellationToken);
        if (duplicate)
            return Result.Failure($"Ward with number '{request.Number}' already exists", 409);

        entity.Number = request.Number;
        entity.Name_En = request.Name_En;
        entity.Name_Alt = request.Name_Alt;
        entity.IsActive = request.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await _repo.UpdateAsync(entity, cancellationToken);
        return Result.Success();
    }
}
