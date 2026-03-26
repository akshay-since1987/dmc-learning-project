using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProposalManagement.Domain.Entities;

namespace ProposalManagement.Infrastructure.Persistence.Configurations;

public class ProposalStepLockConfiguration : IEntityTypeConfiguration<ProposalStepLock>
{
    public void Configure(EntityTypeBuilder<ProposalStepLock> builder)
    {
        builder.HasKey(e => e.Id);

        builder.HasIndex(e => new { e.ProposalId, e.StepNumber })
            .HasFilter("[IsReleased] = 0");

        builder.HasIndex(e => e.ExpiresAt);

        builder.HasOne(e => e.Proposal)
            .WithMany()
            .HasForeignKey(e => e.ProposalId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.LockedBy)
            .WithMany()
            .HasForeignKey(e => e.LockedById)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
