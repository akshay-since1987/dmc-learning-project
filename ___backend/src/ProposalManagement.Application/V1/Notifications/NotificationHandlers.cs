using MediatR;
using Microsoft.EntityFrameworkCore;
using ProposalManagement.Application.Common.Interfaces;
using ProposalManagement.Application.Common.Models;
using ProposalManagement.Domain.Entities;

namespace ProposalManagement.Application.V1.Notifications;

// ── DTOs ──────────────────────────────────────────────

public record NotificationDto(
    long Id,
    string Title_En,
    string Title_Alt,
    string Message_En,
    string Message_Alt,
    string? LinkUrl,
    bool IsRead,
    DateTime CreatedAt);

public record UnreadCountDto(int Count);

// ── Get Notifications ─────────────────────────────────

public record GetNotificationsQuery(int PageSize = 20) : IRequest<List<NotificationDto>>;

public class GetNotificationsQueryHandler : IRequestHandler<GetNotificationsQuery, List<NotificationDto>>
{
    private readonly IRepository<InAppNotification> _repo;
    private readonly ICurrentUser _currentUser;

    public GetNotificationsQueryHandler(IRepository<InAppNotification> repo, ICurrentUser currentUser)
    {
        _repo = repo;
        _currentUser = currentUser;
    }

    public async Task<List<NotificationDto>> Handle(GetNotificationsQuery request, CancellationToken ct)
    {
        return await _repo.Query().AsNoTracking()
            .Where(n => n.UserId == _currentUser.UserId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(request.PageSize)
            .Select(n => new NotificationDto(
                n.Id, n.Title_En, n.Title_Alt,
                n.Message_En, n.Message_Alt,
                n.LinkUrl, n.IsRead, n.CreatedAt))
            .ToListAsync(ct);
    }
}

// ── Get Unread Count ──────────────────────────────────

public record GetUnreadCountQuery : IRequest<UnreadCountDto>;

public class GetUnreadCountQueryHandler : IRequestHandler<GetUnreadCountQuery, UnreadCountDto>
{
    private readonly IRepository<InAppNotification> _repo;
    private readonly ICurrentUser _currentUser;

    public GetUnreadCountQueryHandler(IRepository<InAppNotification> repo, ICurrentUser currentUser)
    {
        _repo = repo;
        _currentUser = currentUser;
    }

    public async Task<UnreadCountDto> Handle(GetUnreadCountQuery request, CancellationToken ct)
    {
        var count = await _repo.Query()
            .CountAsync(n => n.UserId == _currentUser.UserId && !n.IsRead, ct);
        return new UnreadCountDto(count);
    }
}

// ── Mark as Read ──────────────────────────────────────

public record MarkNotificationReadCommand(long Id) : IRequest<Result>;

public class MarkNotificationReadCommandHandler : IRequestHandler<MarkNotificationReadCommand, Result>
{
    private readonly IRepository<InAppNotification> _repo;
    private readonly ICurrentUser _currentUser;

    public MarkNotificationReadCommandHandler(IRepository<InAppNotification> repo, ICurrentUser currentUser)
    {
        _repo = repo;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(MarkNotificationReadCommand request, CancellationToken ct)
    {
        var notification = await _repo.Query()
            .FirstOrDefaultAsync(n => n.Id == request.Id && n.UserId == _currentUser.UserId, ct);

        if (notification is null)
            return Result.NotFound();

        notification.IsRead = true;
        await _repo.UpdateAsync(notification, ct);
        return Result.Success();
    }
}

// ── Mark All as Read ──────────────────────────────────

public record MarkAllNotificationsReadCommand : IRequest<Result>;

public class MarkAllNotificationsReadHandler : IRequestHandler<MarkAllNotificationsReadCommand, Result>
{
    private readonly IRepository<InAppNotification> _repo;
    private readonly ICurrentUser _currentUser;

    public MarkAllNotificationsReadHandler(IRepository<InAppNotification> repo, ICurrentUser currentUser)
    {
        _repo = repo;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(MarkAllNotificationsReadCommand request, CancellationToken ct)
    {
        var unread = await _repo.Query()
            .Where(n => n.UserId == _currentUser.UserId && !n.IsRead)
            .ToListAsync(ct);

        foreach (var n in unread)
            n.IsRead = true;

        foreach (var n in unread)
            await _repo.UpdateAsync(n, ct);

        return Result.Success();
    }
}
