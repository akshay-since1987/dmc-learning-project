using MediatR;
using Microsoft.EntityFrameworkCore;
using ProposalManagement.Application.Common.Interfaces;
using ProposalManagement.Application.Workflow.DTOs;
using ProposalManagement.Domain.Entities;
using ProposalManagement.Domain.Enums;

namespace ProposalManagement.Application.Workflow.Queries;

public class GetApprovalHistoryQueryHandler : IRequestHandler<GetApprovalHistoryQuery, IReadOnlyList<StageHistoryDto>>
{
    private readonly IRepository<Proposal> _proposalRepo;
    private readonly IRepository<ProposalStageHistory> _historyRepo;
    private readonly ICurrentUser _currentUser;

    public GetApprovalHistoryQueryHandler(
        IRepository<Proposal> proposalRepo,
        IRepository<ProposalStageHistory> historyRepo,
        ICurrentUser currentUser)
    {
        _proposalRepo = proposalRepo;
        _historyRepo = historyRepo;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<StageHistoryDto>> Handle(GetApprovalHistoryQuery request, CancellationToken cancellationToken)
    {
        // Verify user has access to this proposal's history
        var proposal = await _proposalRepo.Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.ProposalId, cancellationToken);

        if (proposal is null)
            return [];

        var role = _currentUser.Role;

        // Commissioner, Auditor, Lotus can view any proposal's history
        // Proposer can view their own proposal's history
        // Current stage handler can view the proposal's history
        bool canView = role is UserRole.Commissioner or UserRole.Auditor or UserRole.Lotus
            || proposal.SubmittedById == _currentUser.UserId
            || IsCurrentStageHandler(role, proposal.CurrentStage);

        if (!canView) return [];

        var history = await _historyRepo.Query()
            .AsNoTracking()
            .Where(h => h.ProposalId == request.ProposalId)
            .OrderBy(h => h.CreatedAt)
            .Select(h => new StageHistoryDto(
                h.Id,
                h.FromStage.ToString(),
                h.ToStage.ToString(),
                h.Action.ToString(),
                h.ActionById,
                h.ActionByName_En,
                h.ActionByName_Alt,
                h.ActionByDesignation_En,
                h.ActionByDesignation_Alt,
                h.Reason_En,
                h.Reason_Alt,
                h.Opinion_En,
                h.Opinion_Alt,
                h.Remarks_En,
                h.Remarks_Alt,
                h.DscSignatureRef,
                h.DscSignedAt,
                h.PushedBackToStage != null ? h.PushedBackToStage.ToString() : null,
                h.CreatedAt))
            .ToListAsync(cancellationToken);

        return history;
    }

    private static bool IsCurrentStageHandler(UserRole role, ProposalStage stage) => (role, stage) switch
    {
        (UserRole.CityEngineer, ProposalStage.AtCityEngineer) => true,
        (UserRole.ADO, ProposalStage.AtADO) => true,
        (UserRole.ChiefAccountant, ProposalStage.AtChiefAccountant) => true,
        (UserRole.DeputyCommissioner, ProposalStage.AtDeputyCommissioner) => true,
        (UserRole.Commissioner, ProposalStage.AtCommissioner) => true,
        _ => false
    };
}
