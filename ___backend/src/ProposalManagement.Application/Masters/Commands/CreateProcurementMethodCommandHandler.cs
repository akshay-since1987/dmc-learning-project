using MediatR;
using Microsoft.EntityFrameworkCore;
using ProposalManagement.Application.Common.Interfaces;
using ProposalManagement.Application.Common.Models;
using ProposalManagement.Domain.Entities;

namespace ProposalManagement.Application.Masters.Commands;

public class CreateProcurementMethodCommandHandler : IRequestHandler<CreateProcurementMethodCommand, Result<Guid>>
{
    private readonly IRepository<ProcurementMethod> _repo;

    public CreateProcurementMethodCommandHandler(IRepository<ProcurementMethod> repo)
    {
        _repo = repo;
    }

    public async Task<Result<Guid>> Handle(CreateProcurementMethodCommand request, CancellationToken cancellationToken)
    {
        var exists = await _repo.Query()
            .AnyAsync(d => d.Name_En == request.Name_En, cancellationToken);
        if (exists)
            return Result<Guid>.Failure($"Procurement method '{request.Name_En}' already exists", 409);

        var entity = new ProcurementMethod
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
