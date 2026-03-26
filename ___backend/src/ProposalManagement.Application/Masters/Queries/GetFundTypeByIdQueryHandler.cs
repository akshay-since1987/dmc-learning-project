using MediatR;
using Microsoft.EntityFrameworkCore;
using ProposalManagement.Application.Common.Interfaces;
using ProposalManagement.Application.Common.Models;
using ProposalManagement.Application.Masters.DTOs;
using ProposalManagement.Domain.Entities;

namespace ProposalManagement.Application.Masters.Queries;

public class GetFundTypeByIdQueryHandler : IRequestHandler<GetFundTypeByIdQuery, Result<MasterDetailDto>>
{
    private readonly IRepository<FundType> _repo;

    public GetFundTypeByIdQueryHandler(IRepository<FundType> repo)
    {
        _repo = repo;
    }

    public async Task<Result<MasterDetailDto>> Handle(GetFundTypeByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _repo.Query().AsNoTracking()
            .Where(d => d.Id == request.Id)
            .Select(d => new MasterDetailDto(d.Id, d.Name_En, d.Name_Alt, d.Code, d.IsActive, d.CreatedAt, d.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken);

        if (entity is null)
            return Result<MasterDetailDto>.Failure("Fund type not found", 404);

        return Result<MasterDetailDto>.Success(entity);
    }
}
