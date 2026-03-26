using MediatR;
using Microsoft.EntityFrameworkCore;
using ProposalManagement.Application.Common.Interfaces;
using ProposalManagement.Application.Common.Models;
using ProposalManagement.Domain.Entities;
using ProposalManagement.Domain.Enums;

namespace ProposalManagement.Application.Workflow.Commands;

public class ApproveStageCommandHandler : IRequestHandler<ApproveStageCommand, Result<long>>
{
    private readonly IRepository<Proposal> _repo;
    private readonly IRepository<ProposalStageHistory> _historyRepo;
    private readonly IRepository<User> _userRepo;
    private readonly ICurrentUser _currentUser;
    private readonly IAuditService _auditService;

    public ApproveStageCommandHandler(
        IRepository<Proposal> repo,
        IRepository<ProposalStageHistory> historyRepo,
        IRepository<User> userRepo,
        ICurrentUser currentUser,
        IAuditService auditService)
    {
        _repo = repo;
        _historyRepo = historyRepo;
        _userRepo = userRepo;
        _currentUser = currentUser;
        _auditService = auditService;
    }

    /// <summary>
    /// Maps each stage to the role that is authorized to approve it
    /// and the next stage after approval.
    /// </summary>
    private static readonly Dictionary<ProposalStage, (UserRole AllowedRole, ProposalStage NextStage)> ApprovalMap = new()
    {
        [ProposalStage.AtCityEngineer] = (UserRole.CityEngineer, ProposalStage.AtChiefAccountant),
        [ProposalStage.AtADO] = (UserRole.ADO, ProposalStage.AtChiefAccountant),
        [ProposalStage.AtChiefAccountant] = (UserRole.ChiefAccountant, ProposalStage.AtDeputyCommissioner),
        [ProposalStage.AtDeputyCommissioner] = (UserRole.DeputyCommissioner, ProposalStage.AtCommissioner),
        [ProposalStage.AtCommissioner] = (UserRole.Commissioner, ProposalStage.Approved),
    };

    public async Task<Result<long>> Handle(ApproveStageCommand request, CancellationToken cancellationToken)
    {
        if (!request.TermsAccepted)
            return Result<long>.Failure("You must agree to the terms before approval");

        var entity = await _repo.Query()
            .FirstOrDefaultAsync(p => p.Id == request.ProposalId, cancellationToken);

        if (entity is null)
            return Result<long>.NotFound("Proposal not found");

        if (!ApprovalMap.TryGetValue(entity.CurrentStage, out var mapping))
            return Result<long>.Failure($"Proposal at stage '{entity.CurrentStage}' cannot be approved");

        // Lotus can approve at any stage
        if (_currentUser.Role != UserRole.Lotus && _currentUser.Role != mapping.AllowedRole)
            return Result<long>.Forbidden($"Only {mapping.AllowedRole} can approve at stage {entity.CurrentStage}");

        var user = await _userRepo.Query()
            .Include(u => u.Designation)
            .FirstOrDefaultAsync(u => u.Id == _currentUser.UserId, cancellationToken);

        var fromStage = entity.CurrentStage;
        entity.CurrentStage = mapping.NextStage;
        entity.UpdatedAt = DateTime.UtcNow;
        await _repo.UpdateAsync(entity, cancellationToken);

        var history = new ProposalStageHistory
        {
            ProposalId = entity.Id,
            FromStage = fromStage,
            ToStage = mapping.NextStage,
            Action = WorkflowAction.Approve,
            ActionById = _currentUser.UserId,
            ActionByName_En = user?.FullName_En ?? _currentUser.UserName,
            ActionByName_Alt = user?.FullName_Alt ?? string.Empty,
            ActionByDesignation_En = user?.Designation?.Name_En ?? string.Empty,
            ActionByDesignation_Alt = user?.Designation?.Name_Alt ?? string.Empty,
            Opinion_En = request.Opinion_En,
            Opinion_Alt = request.Opinion_Alt,
            Remarks_En = request.Remarks_En,
            Remarks_Alt = request.Remarks_Alt,
            CreatedAt = DateTime.UtcNow
        };

        await _historyRepo.AddAsync(history, cancellationToken);

        await _auditService.LogAsync(
            AuditAction.Approve, "Proposal", entity.Id.ToString(),
            $"Proposal {entity.ProposalNumber} approved from {fromStage} to {mapping.NextStage} by {_currentUser.UserName}",
            AuditModule.Workflow, AuditSeverity.Info,
            cancellationToken: cancellationToken);

        return Result<long>.Success(history.Id);
    }
}
