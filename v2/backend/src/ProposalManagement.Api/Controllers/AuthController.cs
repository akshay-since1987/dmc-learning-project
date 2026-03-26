using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProposalManagement.Application.Auth.Commands;
using ProposalManagement.Application.Auth.Queries;

namespace ProposalManagement.Api.Controllers;

public class AuthController : BaseController
{
    [HttpPost("send-otp")]
    public async Task<IActionResult> SendOtp([FromBody] SendOtpCommand command)
        => ToActionResult(await Mediator.Send(command));

    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpCommand command)
        => ToActionResult(await Mediator.Send(command));

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand command)
        => ToActionResult(await Mediator.Send(command));

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
        => ToActionResult(await Mediator.Send(new GetMeQuery()));
}
