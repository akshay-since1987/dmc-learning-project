using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProposalManagement.Application.Notifications;

namespace ProposalManagement.Api.Controllers;

[Authorize]
public class NotificationsController : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetMy([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        => ToActionResult(await Mediator.Send(new GetMyNotificationsQuery(page, pageSize)));

    [HttpGet("unread-count")]
    public async Task<IActionResult> UnreadCount()
        => ToActionResult(await Mediator.Send(new GetUnreadCountQuery()));

    [HttpPost("{id:long}/read")]
    public async Task<IActionResult> MarkRead(long id)
        => ToActionResult(await Mediator.Send(new MarkNotificationReadCommand(id)));

    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllRead()
        => ToActionResult(await Mediator.Send(new MarkAllNotificationsReadCommand()));
}
