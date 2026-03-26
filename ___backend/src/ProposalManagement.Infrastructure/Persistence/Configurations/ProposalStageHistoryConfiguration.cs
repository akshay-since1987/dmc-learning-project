using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProposalManagement.Domain.Entities;

namespace ProposalManagement.Infrastructure.Persistence.Configurations;

public class ProposalStageHistoryConfiguration : IEntityTypeConfiguration<ProposalStageHistory>
{
    public void Configure(EntityTypeBuilder<ProposalStageHistory> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).UseIdentityColumn();
        builder.Property(e => e.FromStage).HasMaxLength(50).HasConversion<string>().IsRequired();
        builder.Property(e => e.ToStage).HasMaxLength(50).HasConversion<string>().IsRequired();
        builder.Property(e => e.Action).HasMaxLength(50).HasConversion<string>().IsRequired();
        builder.Property(e => e.ActionByName_En).HasMaxLength(200).IsRequired();
        builder.Property(e => e.ActionByName_Alt).HasMaxLength(200).IsRequired();
        builder.Property(e => e.ActionByDesignation_En).HasMaxLength(200);
        builder.Property(e => e.ActionByDesignation_Alt).HasMaxLength(200);
        builder.Property(e => e.DscSignatureRef).HasMaxLength(500);
        builder.Property(e => e.PushedBackToStage).HasMaxLength(50).HasConversion<string>();

        builder.HasIndex(e => e.ProposalId);
        builder.HasIndex(e => e.CreatedAt);

        builder.HasOne(e => e.Proposal)
            .WithMany(p => p.StageHistory)
            .HasForeignKey(e => e.ProposalId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.ActionBy)
            .WithMany()
            .HasForeignKey(e => e.ActionById)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
