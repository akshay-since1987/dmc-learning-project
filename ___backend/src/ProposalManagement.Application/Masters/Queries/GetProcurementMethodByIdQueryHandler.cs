using MediatR;
using Microsoft.EntityFrameworkCore;
using ProposalManagement.Application.Common.Interfaces;
using ProposalManagement.Application.Common.Models;
using ProposalManagement.Application.Masters.DTOs;
using ProposalManagement.Domain.Entities;

namespace ProposalManagement.Application.Masters.Queries;

public class GetProcurementMethodByIdQueryHandler : IRequestHandler<GetProcurementMethodByIdQuery, Result<MasterDetailDto>>
{
    private readonly IRepository<ProcurementMethod> _repo;

    public GetProcurementMethodByIdQueryHandler(IRepository<ProcurementMethod> repo)
    {
        _repo = repo;
    }

    public async Task<Result<MasterDetailDto>> Handle(GetProcurementMethodByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _repo.Query().AsNoTracking()
            .Where(d => d.Id == request.Id)
            .Select(d => new MasterDetailDto(d.Id, d.Name_En, d.Name_Alt, null, d.IsActive, d.CreatedAt, d.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken);

        if (entity is null)
            return Result<MasterDetailDto>.Failure("Procurement method not found", 404);

        return Result<MasterDetailDto>.Success(entity);
    }
}
