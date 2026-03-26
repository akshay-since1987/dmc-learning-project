using MediatR;
using Microsoft.EntityFrameworkCore;
using ProposalManagement.Application.Common.Interfaces;
using ProposalManagement.Application.Common.Models;
using ProposalManagement.Domain.Entities;
using ProposalManagement.Domain.Enums;

namespace ProposalManagement.Application.Proposals.Commands;

public class DeleteProposalCommandHandler : IRequestHandler<DeleteProposalCommand, Result>
{
    private readonly IRepository<Proposal> _repo;
    private readonly ICurrentUser _currentUser;
    private readonly IAuditService _auditService;

    public DeleteProposalCommandHandler(
        IRepository<Proposal> repo,
        ICurrentUser currentUser,
        IAuditService auditService)
    {
        _repo = repo;
        _currentUser = currentUser;
        _auditService = auditService;
    }

    public async Task<Result> Handle(DeleteProposalCommand request, CancellationToken cancellationToken)
    {
        var isLotus = _currentUser.Role == UserRole.Lotus;

        var entity = isLotus
            ? await _repo.QueryIgnoreFilters().FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            : await _repo.Query().FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (entity is null)
            return Result.NotFound("Proposal not found");

        if (!isLotus)
        {
            if (entity.SubmittedById != _currentUser.UserId)
                return Result.Forbidden("Only the proposer can delete this proposal");

            if (entity.CurrentStage != ProposalStage.Draft)
                return Result.Failure("Proposal can only be deleted in Draft stage");
        }

        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;
        await _repo.UpdateAsync(entity, cancellationToken);

        await _auditService.LogAsync(
            AuditAction.Delete, "Proposal", entity.Id.ToString(),
            $"Proposal {entity.ProposalNumber} soft-deleted",
            AuditModule.Proposal, AuditSeverity.Warning,
            cancellationToken: cancellationToken);

        return Result.Success();
    }
}
