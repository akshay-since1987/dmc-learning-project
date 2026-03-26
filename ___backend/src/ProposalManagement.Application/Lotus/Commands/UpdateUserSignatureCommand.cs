using MediatR;
using Microsoft.EntityFrameworkCore;
using ProposalManagement.Application.Common.Interfaces;
using ProposalManagement.Application.Common.Models;
using ProposalManagement.Domain.Entities;

namespace ProposalManagement.Application.Lotus.Commands;

public record UpdateUserSignatureCommand(Guid UserId, string SignaturePath) : IRequest<Result>;

public class UpdateUserSignatureCommandHandler : IRequestHandler<UpdateUserSignatureCommand, Result>
{
    private readonly IRepository<User> _repo;

    public UpdateUserSignatureCommandHandler(IRepository<User> repo) => _repo = repo;

    public async Task<Result> Handle(UpdateUserSignatureCommand request, CancellationToken cancellationToken)
    {
        var user = await _repo.QueryIgnoreFilters()
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        if (user is null)
            return Result.Failure("User not found", 404);

        user.SignaturePath = request.SignaturePath;
        user.UpdatedAt = DateTime.UtcNow;
        await _repo.UpdateAsync(user, cancellationToken);
        return Result.Success();
    }
}
