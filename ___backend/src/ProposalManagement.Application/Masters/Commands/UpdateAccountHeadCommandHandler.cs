using MediatR;
using Microsoft.EntityFrameworkCore;
using ProposalManagement.Application.Common.Interfaces;
using ProposalManagement.Application.Common.Models;
using ProposalManagement.Domain.Entities;

namespace ProposalManagement.Application.Masters.Commands;

public class UpdateAccountHeadCommandHandler : IRequestHandler<UpdateAccountHeadCommand, Result>
{
    private readonly IRepository<AccountHead> _repo;

    public UpdateAccountHeadCommandHandler(IRepository<AccountHead> repo)
    {
        _repo = repo;
    }

    public async Task<Result> Handle(UpdateAccountHeadCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repo.GetByIdAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result.Failure("Account head not found", 404);

        var duplicate = await _repo.Query()
            .AnyAsync(d => d.Code == request.Code && d.Id != request.Id, cancellationToken);
        if (duplicate)
            return Result.Failure($"Account head with code '{request.Code}' already exists", 409);

        entity.Code = request.Code;
        entity.Name_En = request.Name_En;
        entity.Name_Alt = request.Name_Alt;
        entity.IsActive = request.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await _repo.UpdateAsync(entity, cancellationToken);
        return Result.Success();
    }
}
