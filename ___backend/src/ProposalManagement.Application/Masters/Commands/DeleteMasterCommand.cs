using MediatR;
using ProposalManagement.Application.Common.Models;

namespace ProposalManagement.Application.Masters.Commands;

public record DeleteDepartmentCommand(Guid Id) : IRequest<Result>;
public record DeleteDesignationCommand(Guid Id) : IRequest<Result>;
public record DeleteFundTypeCommand(Guid Id) : IRequest<Result>;
public record DeleteAccountHeadCommand(Guid Id) : IRequest<Result>;
public record DeleteWardCommand(Guid Id) : IRequest<Result>;
public record DeleteProcurementMethodCommand(Guid Id) : IRequest<Result>;
