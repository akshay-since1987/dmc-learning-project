using MediatR;
using Microsoft.EntityFrameworkCore;
using ProposalManagement.Application.Common.Interfaces;
using ProposalManagement.Application.Common.Models;
using ProposalManagement.Domain.Entities;

namespace ProposalManagement.Application.Notifications;

// ── DTOs ──
public record NotificationDto(long Id, string Type, string Title_En, string? Title_Mr, string Message_En,
    string? Message_Mr, Guid? ProposalId, bool IsRead, DateTime? ReadAt, DateTime CreatedAt);

// ── Query: Get my notifications ──
public record GetMyNotificationsQuery(int Page = 1, int PageSize = 20) : IRequest<Result<PagedList<NotificationDto>>>;

public record PagedList<T>(List<T> Items, int TotalCount, int Page, int PageSize);

public class GetMyNotificationsHandler(IAppDbContext db, ICurrentUser user) 
    : IRequestHandler<GetMyNotificationsQuery, Result<PagedList<NotificationDto>>>
{
    public async Task<Result<PagedList<NotificationDto>>> Handle(GetMyNotificationsQuery request, CancellationToken ct)
    {
        var uid = user.UserId!.Value;
        var q = db.Notifications.Where(n => n.UserId == uid).OrderByDescending(n => n.CreatedAt);

        var total = await q.CountAsync(ct);
        var items = await q.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize)
            .Select(n => new NotificationDto(n.Id, n.Type, n.Title_En, n.Title_Mr, n.Message_En,
                n.Message_Mr, n.ProposalId, n.IsRead, n.ReadAt, n.CreatedAt))
            .ToListAsync(ct);

        return Result<PagedList<NotificationDto>>.Success(new(items, total, request.Page, request.PageSize));
    }
}

// ── Query: Unread count ──
public record GetUnreadCountQuery : IRequest<Result<int>>;

public class GetUnreadCountHandler(IAppDbContext db, ICurrentUser user) : IRequestHandler<GetUnreadCountQuery, Result<int>>
{
    public async Task<Result<int>> Handle(GetUnreadCountQuery request, CancellationToken ct)
    {
        var count = await db.Notifications.CountAsync(n => n.UserId == user.UserId && !n.IsRead, ct);
        return Result<int>.Success(count);
    }
}

// ── Command: Mark as read ──
public record MarkNotificationReadCommand(long Id) : IRequest<Result>;

public class MarkNotificationReadHandler(IAppDbContext db, ICurrentUser user) : IRequestHandler<MarkNotificationReadCommand, Result>
{
    public async Task<Result> Handle(MarkNotificationReadCommand request, CancellationToken ct)
    {
        var n = await db.Notifications.FirstOrDefaultAsync(x => x.Id == request.Id && x.UserId == user.UserId, ct);
        if (n is null) return Result.NotFound();
        n.IsRead = true;
        n.ReadAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

// ── Command: Mark all as read ──
public record MarkAllNotificationsReadCommand : IRequest<Result>;

public class MarkAllNotificationsReadHandler(IAppDbContext db, ICurrentUser user) : IRequestHandler<MarkAllNotificationsReadCommand, Result>
{
    public async Task<Result> Handle(MarkAllNotificationsReadCommand request, CancellationToken ct)
    {
        var unread = await db.Notifications.Where(n => n.UserId == user.UserId && !n.IsRead).ToListAsync(ct);
        foreach (var n in unread) { n.IsRead = true; n.ReadAt = DateTime.UtcNow; }
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

// ── Service: Create notifications (used by workflow handlers) ──
public interface INotificationService
{
    Task NotifyAsync(Guid userId, Guid palikaId, string type, string title_En, string message_En,
        Guid? proposalId = null, CancellationToken ct = default);
}

public class NotificationService(IAppDbContext db) : INotificationService
{
    public async Task NotifyAsync(Guid userId, Guid palikaId, string type, string title_En, string message_En,
        Guid? proposalId = null, CancellationToken ct = default)
    {
        db.Notifications.Add(new Notification
        {
            UserId = userId, PalikaId = palikaId, ProposalId = proposalId,
            Type = type, Title_En = title_En, Message_En = message_En,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync(ct);
    }
}
