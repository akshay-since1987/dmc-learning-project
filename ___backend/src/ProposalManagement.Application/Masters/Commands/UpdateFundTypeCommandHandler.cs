using MediatR;
using Microsoft.EntityFrameworkCore;
using ProposalManagement.Application.Common.Interfaces;
using ProposalManagement.Application.Common.Models;
using ProposalManagement.Domain.Entities;

namespace ProposalManagement.Application.Masters.Commands;

public class UpdateFundTypeCommandHandler : IRequestHandler<UpdateFundTypeCommand, Result>
{
    private readonly IRepository<FundType> _repo;

    public UpdateFundTypeCommandHandler(IRepository<FundType> repo)
    {
        _repo = repo;
    }

    public async Task<Result> Handle(UpdateFundTypeCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repo.GetByIdAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result.Failure("Fund type not found", 404);

        var duplicate = await _repo.Query()
            .AnyAsync(d => d.Code == request.Code && d.Id != request.Id, cancellationToken);
        if (duplicate)
            return Result.Failure($"Fund type with code '{request.Code}' already exists", 409);

        entity.Name_En = request.Name_En;
        entity.Name_Alt = request.Name_Alt;
        entity.Code = request.Code;
        entity.IsActive = request.IsActive;
        entity.IsMnp = request.IsMnp;
        entity.IsState = request.IsState;
        entity.IsCentral = request.IsCentral;
        entity.IsDpdc = request.IsDpdc;
        entity.UpdatedAt = DateTime.UtcNow;

        await _repo.UpdateAsync(entity, cancellationToken);
        return Result.Success();
    }
}
