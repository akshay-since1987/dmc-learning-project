using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProposalManagement.Application.Masters.Queries;

namespace ProposalManagement.Api.Controllers;

[Authorize]
public class MastersController : BaseController
{
    [HttpGet("departments")]
    public async Task<IActionResult> GetDepartments([FromQuery] Guid palikaId)
        => ToActionResult(await Mediator.Send(new GetDepartmentsQuery(palikaId)));

    [HttpGet("zones")]
    public async Task<IActionResult> GetZones([FromQuery] Guid palikaId)
        => ToActionResult(await Mediator.Send(new GetZonesQuery(palikaId)));

    [HttpGet("prabhags")]
    public async Task<IActionResult> GetPrabhags([FromQuery] Guid palikaId, [FromQuery] Guid? zoneId)
        => ToActionResult(await Mediator.Send(new GetPrabhagsQuery(palikaId, zoneId)));

    [HttpGet("designations")]
    public async Task<IActionResult> GetDesignations([FromQuery] Guid palikaId)
        => ToActionResult(await Mediator.Send(new GetDesignationsQuery(palikaId)));

    [HttpGet("fund-types")]
    public async Task<IActionResult> GetFundTypes([FromQuery] Guid palikaId)
        => ToActionResult(await Mediator.Send(new GetFundTypesQuery(palikaId)));

    [HttpGet("work-methods")]
    public async Task<IActionResult> GetWorkMethods([FromQuery] Guid palikaId)
        => ToActionResult(await Mediator.Send(new GetWorkMethodsQuery(palikaId)));

    [HttpGet("site-conditions")]
    public async Task<IActionResult> GetSiteConditions()
        => ToActionResult(await Mediator.Send(new GetSiteConditionsQuery()));

    [HttpGet("request-sources")]
    public async Task<IActionResult> GetRequestSources([FromQuery] Guid palikaId)
        => ToActionResult(await Mediator.Send(new GetRequestSourcesQuery(palikaId)));

    [HttpGet("work-categories")]
    public async Task<IActionResult> GetWorkCategories([FromQuery] Guid? departmentId)
        => ToActionResult(await Mediator.Send(new GetWorkCategoriesQuery(departmentId)));

    [HttpGet("budget-heads")]
    public async Task<IActionResult> GetBudgetHeads([FromQuery] Guid palikaId)
        => ToActionResult(await Mediator.Send(new GetBudgetHeadsQuery(palikaId)));

    [HttpGet("users-by-role")]
    public async Task<IActionResult> GetUsersByRole([FromQuery] string role, [FromQuery] Guid palikaId)
        => ToActionResult(await Mediator.Send(new GetUsersByRoleQuery(role, palikaId)));
}
