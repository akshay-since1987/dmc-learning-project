using MediatR;
using Microsoft.EntityFrameworkCore;
using ProposalManagement.Application.Common.Interfaces;
using ProposalManagement.Application.Common.Models;
using ProposalManagement.Domain.Entities;

namespace ProposalManagement.Application.Masters.Commands;

public class CreateAccountHeadCommandHandler : IRequestHandler<CreateAccountHeadCommand, Result<Guid>>
{
    private readonly IRepository<AccountHead> _repo;

    public CreateAccountHeadCommandHandler(IRepository<AccountHead> repo)
    {
        _repo = repo;
    }

    public async Task<Result<Guid>> Handle(CreateAccountHeadCommand request, CancellationToken cancellationToken)
    {
        var exists = await _repo.Query().AnyAsync(d => d.Code == request.Code, cancellationToken);
        if (exists)
            return Result<Guid>.Failure($"Account head with code '{request.Code}' already exists", 409);

        var entity = new AccountHead
        {
            Id = Guid.NewGuid(),
            Code = request.Code,
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
