using MediatR;
using Microsoft.EntityFrameworkCore;
using ProposalManagement.Application.Common.Interfaces;
using ProposalManagement.Application.Common.Models;
using ProposalManagement.Domain.Entities;

namespace ProposalManagement.Application.Masters.Commands;

public class CreateFundTypeCommandHandler : IRequestHandler<CreateFundTypeCommand, Result<Guid>>
{
    private readonly IRepository<FundType> _repo;

    public CreateFundTypeCommandHandler(IRepository<FundType> repo)
    {
        _repo = repo;
    }

    public async Task<Result<Guid>> Handle(CreateFundTypeCommand request, CancellationToken cancellationToken)
    {
        var exists = await _repo.Query().AnyAsync(d => d.Code == request.Code, cancellationToken);
        if (exists)
            return Result<Guid>.Failure($"Fund type with code '{request.Code}' already exists", 409);

        var entity = new FundType
        {
            Id = Guid.NewGuid(),
            Name_En = request.Name_En,
            Name_Alt = request.Name_Alt,
            Code = request.Code,
            IsMnp = request.IsMnp,
            IsState = request.IsState,
            IsCentral = request.IsCentral,
            IsDpdc = request.IsDpdc,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _repo.AddAsync(entity, cancellationToken);
        return Result<Guid>.Success(entity.Id);
    }
}
