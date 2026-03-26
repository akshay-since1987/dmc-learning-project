using MediatR;
using ProposalManagement.Application.Common.Models;

namespace ProposalManagement.Application.Auth.Commands;

public record SendOtpCommand(string MobileNumber) : IRequest<Result>;
