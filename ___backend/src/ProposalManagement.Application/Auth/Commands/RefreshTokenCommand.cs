using MediatR;
using ProposalManagement.Application.Common.Models;

namespace ProposalManagement.Application.Auth.Commands;

public record RefreshTokenCommand(string RefreshToken) : IRequest<Result<AuthResponse>>;
