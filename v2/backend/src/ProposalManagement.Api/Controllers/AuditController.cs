using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProposalManagement.Application.Audit;

namespace ProposalManagement.Api.Controllers;

[Authorize]
public class AuditController : BaseController
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] string? entityType = null, [FromQuery] string? action = null,
        [FromQuery] string? module = null, [FromQuery] string? userId = null,
        [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null,
        [FromQuery] string? search = null)
        => ToActionResult(await Mediator.Send(new GetAuditTrailQuery
        {
            Page = page, PageSize = pageSize, EntityType = entityType, Action = action,
            Module = module, UserId = userId, From = from, To = to, Search = search
        }));
}
