using MediatR;
using Microsoft.EntityFrameworkCore;
using ProposalManagement.Application.Common.Interfaces;
using ProposalManagement.Application.Common.Models;
using ProposalManagement.Application.Masters.DTOs;
using ProposalManagement.Domain.Entities;

namespace ProposalManagement.Application.Masters.Queries;

public class GetWardByIdQueryHandler : IRequestHandler<GetWardByIdQuery, Result<MasterDetailDto>>
{
    private readonly IRepository<Ward> _repo;

    public GetWardByIdQueryHandler(IRepository<Ward> repo)
    {
        _repo = repo;
    }

    public async Task<Result<MasterDetailDto>> Handle(GetWardByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _repo.Query().AsNoTracking()
            .Where(d => d.Id == request.Id)
            .Select(d => new MasterDetailDto(d.Id, d.Name_En, d.Name_Alt, d.Number.ToString(), d.IsActive, d.CreatedAt, d.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken);

        if (entity is null)
            return Result<MasterDetailDto>.Failure("Ward not found", 404);

        return Result<MasterDetailDto>.Success(entity);
    }
}
