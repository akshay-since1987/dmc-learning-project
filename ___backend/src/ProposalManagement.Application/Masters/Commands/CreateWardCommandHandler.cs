using MediatR;
using Microsoft.EntityFrameworkCore;
using ProposalManagement.Application.Common.Interfaces;
using ProposalManagement.Application.Common.Models;
using ProposalManagement.Domain.Entities;

namespace ProposalManagement.Application.Masters.Commands;

public class CreateWardCommandHandler : IRequestHandler<CreateWardCommand, Result<Guid>>
{
    private readonly IRepository<Ward> _repo;

    public CreateWardCommandHandler(IRepository<Ward> repo)
    {
        _repo = repo;
    }

    public async Task<Result<Guid>> Handle(CreateWardCommand request, CancellationToken cancellationToken)
    {
        var exists = await _repo.Query().AnyAsync(d => d.Number == request.Number, cancellationToken);
        if (exists)
            return Result<Guid>.Failure($"Ward with number '{request.Number}' already exists", 409);

        var entity = new Ward
        {
            Id = Guid.NewGuid(),
            Number = request.Number,
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
