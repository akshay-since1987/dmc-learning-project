using MediatR;
using Microsoft.EntityFrameworkCore;
using ProposalManagement.Application.Common.Interfaces;
using ProposalManagement.Application.Common.Models;
using ProposalManagement.Domain.Entities;

namespace ProposalManagement.Application.V1.StepLocks;

// ── DTOs ──────────────────────────────────────────────

public record StepLockDto(
    Guid Id,
    int StepNumber,
    Guid LockedById,
    string LockedByName,
    DateTime LockedAt,
    DateTime ExpiresAt);

// ── Acquire Lock ──────────────────────────────────────

public record AcquireStepLockCommand(Guid ProposalId, int StepNumber) : IRequest<Result<StepLockDto>>;

public class AcquireStepLockCommandHandler : IRequestHandler<AcquireStepLockCommand, Result<StepLockDto>>
{
    private readonly IRepository<ProposalStepLock> _lockRepo;
    private readonly IRepository<User> _userRepo;
    private readonly ICurrentUser _currentUser;

    public AcquireStepLockCommandHandler(
        IRepository<ProposalStepLock> lockRepo,
        IRepository<User> userRepo,
        ICurrentUser currentUser)
    {
        _lockRepo = lockRepo;
        _userRepo = userRepo;
        _currentUser = currentUser;
    }

    public async Task<Result<StepLockDto>> Handle(AcquireStepLockCommand request, CancellationToken ct)
    {
        var now = DateTime.UtcNow;

        // Check for existing active lock
        var existing = await _lockRepo.Query()
            .Where(l => l.ProposalId == request.ProposalId
                        && l.StepNumber == request.StepNumber
                        && !l.IsReleased
                        && l.ExpiresAt > now)
            .FirstOrDefaultAsync(ct);

        if (existing != null)
        {
            if (existing.LockedById == _currentUser.UserId)
            {
                // Extend the lock
                existing.ExpiresAt = now.AddHours(2);
                await _lockRepo.UpdateAsync(existing, ct);

                var user = await _userRepo.Query().AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == _currentUser.UserId, ct);

                return Result<StepLockDto>.Success(new StepLockDto(
                    existing.Id, existing.StepNumber, existing.LockedById,
                    user?.FullName_En ?? "", existing.LockedAt, existing.ExpiresAt));
            }

            var locker = await _userRepo.Query().AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == existing.LockedById, ct);

            return Result<StepLockDto>.Failure(
                $"This step is currently being edited by {locker?.FullName_En ?? "another user"}",
                409);
        }

        // Create new lock (2 hours)
        var lockEntity = new ProposalStepLock
        {
            Id = Guid.NewGuid(),
            ProposalId = request.ProposalId,
            StepNumber = request.StepNumber,
            LockedById = _currentUser.UserId,
            LockedAt = now,
            ExpiresAt = now.AddHours(2),
            IsReleased = false
        };

        await _lockRepo.AddAsync(lockEntity, ct);

        var currentUser = await _userRepo.Query().AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == _currentUser.UserId, ct);

        return Result<StepLockDto>.Success(new StepLockDto(
            lockEntity.Id, lockEntity.StepNumber, lockEntity.LockedById,
            currentUser?.FullName_En ?? "", lockEntity.LockedAt, lockEntity.ExpiresAt));
    }
}

// ── Release Lock ──────────────────────────────────────

public record ReleaseStepLockCommand(Guid ProposalId, int StepNumber) : IRequest<Result>;

public class ReleaseStepLockCommandHandler : IRequestHandler<ReleaseStepLockCommand, Result>
{
    private readonly IRepository<ProposalStepLock> _lockRepo;
    private readonly ICurrentUser _currentUser;

    public ReleaseStepLockCommandHandler(IRepository<ProposalStepLock> lockRepo, ICurrentUser currentUser)
    {
        _lockRepo = lockRepo;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(ReleaseStepLockCommand request, CancellationToken ct)
    {
        var existing = await _lockRepo.Query()
            .Where(l => l.ProposalId == request.ProposalId
                        && l.StepNumber == request.StepNumber
                        && l.LockedById == _currentUser.UserId
                        && !l.IsReleased)
            .FirstOrDefaultAsync(ct);

        if (existing is null)
            return Result.Success(); // No lock to release

        existing.IsReleased = true;
        await _lockRepo.UpdateAsync(existing, ct);
        return Result.Success();
    }
}

// ── Check Lock Status ─────────────────────────────────

public record GetStepLockQuery(Guid ProposalId, int StepNumber) : IRequest<StepLockDto?>;

public class GetStepLockQueryHandler : IRequestHandler<GetStepLockQuery, StepLockDto?>
{
    private readonly IRepository<ProposalStepLock> _lockRepo;
    private readonly IRepository<User> _userRepo;

    public GetStepLockQueryHandler(IRepository<ProposalStepLock> lockRepo, IRepository<User> userRepo)
    {
        _lockRepo = lockRepo;
        _userRepo = userRepo;
    }

    public async Task<StepLockDto?> Handle(GetStepLockQuery request, CancellationToken ct)
    {
        var now = DateTime.UtcNow;

        var existing = await _lockRepo.Query().AsNoTracking()
            .Where(l => l.ProposalId == request.ProposalId
                        && l.StepNumber == request.StepNumber
                        && !l.IsReleased
                        && l.ExpiresAt > now)
            .FirstOrDefaultAsync(ct);

        if (existing is null) return null;

        var user = await _userRepo.Query().AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == existing.LockedById, ct);

        return new StepLockDto(
            existing.Id, existing.StepNumber, existing.LockedById,
            user?.FullName_En ?? "", existing.LockedAt, existing.ExpiresAt);
    }
}
