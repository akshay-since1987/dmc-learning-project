using MediatR;
using Microsoft.EntityFrameworkCore;
using ProposalManagement.Application.Common.Interfaces;
using ProposalManagement.Application.Common.Models;
using ProposalManagement.Domain.Entities;

namespace ProposalManagement.Application.Masters.Commands;

public class CreateDesignationCommandHandler : IRequestHandler<CreateDesignationCommand, Result<Guid>>
{
    private readonly IRepository<Designation> _repo;

    public CreateDesignationCommandHandler(IRepository<Designation> repo)
    {
        _repo = repo;
    }

    public async Task<Result<Guid>> Handle(CreateDesignationCommand request, CancellationToken cancellationToken)
    {
        var exists = await _repo.Query()
            .AnyAsync(d => d.Name_En == request.Name_En, cancellationToken);
        if (exists)
            return Result<Guid>.Failure($"Designation '{request.Name_En}' already exists", 409);

        var entity = new Designation
        {
            Id = Guid.NewGuid(),
            Name_En = request.Name_En,
            Name_Alt = request.Name_Alt,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _repo.AddAsync(entity, cancellationToken);
        return Result<Guid>.Success(entity.Id);
    }
}
