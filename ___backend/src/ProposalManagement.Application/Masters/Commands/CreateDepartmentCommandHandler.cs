using MediatR;
using Microsoft.EntityFrameworkCore;
using ProposalManagement.Application.Common.Interfaces;
using ProposalManagement.Application.Common.Models;
using ProposalManagement.Domain.Entities;

namespace ProposalManagement.Application.Masters.Commands;

public class CreateDepartmentCommandHandler : IRequestHandler<CreateDepartmentCommand, Result<Guid>>
{
    private readonly IRepository<Department> _repo;

    public CreateDepartmentCommandHandler(IRepository<Department> repo)
    {
        _repo = repo;
    }

    public async Task<Result<Guid>> Handle(CreateDepartmentCommand request, CancellationToken cancellationToken)
    {
        var exists = await _repo.Query().AnyAsync(d => d.Code == request.Code, cancellationToken);
        if (exists)
            return Result<Guid>.Failure($"Department with code '{request.Code}' already exists", 409);

        var entity = new Department
        {
            Id = Guid.NewGuid(),
            Name_En = request.Name_En,
            Name_Alt = request.Name_Alt,
            Code = request.Code,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _repo.AddAsync(entity, cancellationToken);
        return Result<Guid>.Success(entity.Id);
    }
}
