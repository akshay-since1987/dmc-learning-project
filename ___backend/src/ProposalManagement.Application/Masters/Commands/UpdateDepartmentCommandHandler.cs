using MediatR;
using Microsoft.EntityFrameworkCore;
using ProposalManagement.Application.Common.Interfaces;
using ProposalManagement.Application.Common.Models;
using ProposalManagement.Domain.Entities;

namespace ProposalManagement.Application.Masters.Commands;

public class UpdateDepartmentCommandHandler : IRequestHandler<UpdateDepartmentCommand, Result>
{
    private readonly IRepository<Department> _repo;

    public UpdateDepartmentCommandHandler(IRepository<Department> repo)
    {
        _repo = repo;
    }

    public async Task<Result> Handle(UpdateDepartmentCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repo.GetByIdAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result.Failure("Department not found", 404);

        var duplicate = await _repo.Query()
            .AnyAsync(d => d.Code == request.Code && d.Id != request.Id, cancellationToken);
        if (duplicate)
            return Result.Failure($"Department with code '{request.Code}' already exists", 409);

        entity.Name_En = request.Name_En;
        entity.Name_Alt = request.Name_Alt;
        entity.Code = request.Code;
        entity.IsActive = request.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await _repo.UpdateAsync(entity, cancellationToken);
        return Result.Success();
    }
}
