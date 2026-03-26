using MediatR;
using Microsoft.EntityFrameworkCore;
using ProposalManagement.Application.Common.Interfaces;
using ProposalManagement.Application.Common.Models;
using ProposalManagement.Domain.Entities;
using ProposalManagement.Domain.Enums;

namespace ProposalManagement.Application.Workflow.Commands;

public class PushBackStageCommandHandler : IRequestHandler<PushBackStageCommand, Result>
{
    private readonly IRepository<Proposal> _repo;
    private readonly IRepository<ProposalStageHistory> _historyRepo;
    private readonly IRepository<User> _userRepo;
    private readonly ICurrentUser _currentUser;
    private readonly IAuditService _auditService;

    public PushBackStageCommandHandler(
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
    /// Maps each stage to the role authorized to push back, and valid push-back targets.
    /// </summary>
    private static readonly Dictionary<ProposalStage, (UserRole AllowedRole, ProposalStage[] ValidTargets)> PushBackMap = new()
    {
        [ProposalStage.AtCityEngineer] = (UserRole.CityEngineer, [ProposalStage.Draft]),
        [ProposalStage.AtChiefAccountant] = (UserRole.ChiefAccountant, [ProposalStage.Draft, ProposalStage.AtCityEngineer]),
        [ProposalStage.AtDeputyCommissioner] = (UserRole.DeputyCommissioner, [ProposalStage.Draft, ProposalStage.AtCityEngineer, ProposalStage.AtChiefAccountant]),
        [ProposalStage.AtCommissioner] = (UserRole.Commissioner, [ProposalStage.Draft, ProposalStage.AtCityEngineer, ProposalStage.AtChiefAccountant, ProposalStage.AtDeputyCommissioner]),
    };

    public async Task<Result> Handle(PushBackStageCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repo.Query()
            .FirstOrDefaultAsync(p => p.Id == request.ProposalId, cancellationToken);

        if (entity is null)
            return Result.NotFound("Proposal not found");

        if (!PushBackMap.TryGetValue(entity.CurrentStage, out var mapping))
            return Result.Failure($"Proposal at stage '{entity.CurrentStage}' cannot be pushed back");

        // Lotus can push back at any stage
        if (_currentUser.Role != UserRole.Lotus && _currentUser.Role != mapping.AllowedRole)
            return Result.Forbidden($"Only {mapping.AllowedRole} can push back at stage {entity.CurrentStage}");

        if (!Enum.TryParse<ProposalStage>(request.TargetStage, true, out var targetStage))
            return Result.Failure($"Invalid target stage: {request.TargetStage}");

        // Lotus can push back to any stage; others must follow the valid targets
        if (_currentUser.Role != UserRole.Lotus && !mapping.ValidTargets.Contains(targetStage))
            return Result.Failure($"Cannot push back to {targetStage} from {entity.CurrentStage}");

        var user = await _userRepo.Query()
            .Include(u => u.Designation)
            .FirstOrDefaultAsync(u => u.Id == _currentUser.UserId, cancellationToken);

        var fromStage = entity.CurrentStage;

        // If pushing back to Draft, set PushedBack so the proposer sees it clearly
        entity.CurrentStage = targetStage == ProposalStage.Draft
            ? ProposalStage.PushedBack
            : targetStage;
        entity.PushBackCount++;
        entity.UpdatedAt = DateTime.UtcNow;
        await _repo.UpdateAsync(entity, cancellationToken);

        var history = new ProposalStageHistory
        {
            ProposalId = entity.Id,
            FromStage = fromStage,
            ToStage = entity.CurrentStage,
            Action = WorkflowAction.PushBack,
            ActionById = _currentUser.UserId,
            ActionByName_En = user?.FullName_En ?? _currentUser.UserName,
            ActionByName_Alt = user?.FullName_Alt ?? string.Empty,
            ActionByDesignation_En = user?.Designation?.Name_En ?? string.Empty,
            ActionByDesignation_Alt = user?.Designation?.Name_Alt ?? string.Empty,
            Reason_En = request.Reason_En,
            Reason_Alt = request.Reason_Alt,
            Opinion_En = request.Opinion_En,
            Opinion_Alt = request.Opinion_Alt,
            Remarks_En = request.Remarks_En,
            Remarks_Alt = request.Remarks_Alt,
            PushedBackToStage = targetStage,
            CreatedAt = DateTime.UtcNow
        };

        await _historyRepo.AddAsync(history, cancellationToken);

        await _auditService.LogAsync(
            AuditAction.PushBack, "Proposal", entity.Id.ToString(),
            $"Proposal {entity.ProposalNumber} pushed back from {fromStage} to {targetStage} by {_currentUser.UserName}",
            AuditModule.Workflow, AuditSeverity.Warning,
            cancellationToken: cancellationToken);

        return Result.Success();
    }
}
