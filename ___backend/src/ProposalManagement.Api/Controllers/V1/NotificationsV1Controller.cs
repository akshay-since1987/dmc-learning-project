using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProposalManagement.Application.V1.Notifications;

namespace ProposalManagement.Api.Controllers.V1;

[ApiController]
[Route("api/v1/notifications")]
[Authorize]
public class NotificationsV1Controller : ControllerBase
{
    private readonly IMediator _mediator;
    public NotificationsV1Controller(IMediator mediator) => _mediator = mediator;

    /// <summary>Get current user's notifications</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetNotificationsQuery(pageSize), ct);
        return Ok(result);
    }

    /// <summary>Get unread notification count</summary>
    [HttpGet("unread-count")]
    public async Task<IActionResult> UnreadCount(CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetUnreadCountQuery(), ct);
        return Ok(result);
    }

    /// <summary>Mark a single notification as read</summary>
    [HttpPost("{id:long}/read")]
    public async Task<IActionResult> MarkRead(long id, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new MarkNotificationReadCommand(id), ct);
        return result.IsSuccess ? NoContent() : StatusCode(result.StatusCode, new { result.Error });
    }

    /// <summary>Mark all notifications as read</summary>
    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllRead(CancellationToken ct = default)
    {
        var result = await _mediator.Send(new MarkAllNotificationsReadCommand(), ct);
        return result.IsSuccess ? NoContent() : StatusCode(result.StatusCode, new { result.Error });
    }
}
