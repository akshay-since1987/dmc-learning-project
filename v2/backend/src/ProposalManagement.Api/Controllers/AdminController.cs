using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProposalManagement.Application.Admin;

namespace ProposalManagement.Api.Controllers;

[Authorize(Roles = "Lotus")]
[Route("api/admin")]
public class AdminController : BaseController
{
    // ── Users ──
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] string? role = null, [FromQuery] string? search = null)
        => ToActionResult(await Mediator.Send(new GetUsersQuery(page, pageSize, role, search)));

    [HttpGet("users/{id:guid}")]
    public async Task<IActionResult> GetUser(Guid id)
        => ToActionResult(await Mediator.Send(new GetUserByIdQuery(id)));

    [HttpPost("users")]
    public async Task<IActionResult> CreateUser([FromBody] SaveUserCommand command)
        => ToActionResult(await Mediator.Send(command));

    [HttpPut("users/{id:guid}")]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] SaveUserCommand command)
        => ToActionResult(await Mediator.Send(command with { Id = id }));

    [HttpPost("users/{id:guid}/toggle-active")]
    public async Task<IActionResult> ToggleActive(Guid id)
        => ToActionResult(await Mediator.Send(new ToggleUserActiveCommand(id)));

    [HttpDelete("users/{id:guid}")]
    public async Task<IActionResult> DeleteUser(Guid id)
        => ToActionResult(await Mediator.Send(new DeleteUserCommand(id)));
}
