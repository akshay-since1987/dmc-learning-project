using MediatR;
using ProposalManagement.Application.Common.Models;

namespace ProposalManagement.Application.Auth.Commands;

public record VerifyOtpCommand(string MobileNumber, string Otp, string? Password = null) : IRequest<Result<AuthResponse>>;

public record AuthResponse(string AccessToken, string RefreshToken, UserDto User);

public record UserDto(
    Guid Id,
    string FullName_En,
    string FullName_Alt,
    string MobileNumber,
    string? Email,
    string Role,
    Guid? DepartmentId,
    Guid? DesignationId,
    string? SignaturePath);
