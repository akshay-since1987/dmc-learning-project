using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProposalManagement.Domain.Entities;

namespace ProposalManagement.Infrastructure.Persistence.Configurations;

public class ProposalConfiguration : IEntityTypeConfiguration<Proposal>
{
    public void Configure(EntityTypeBuilder<Proposal> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.ProposalNumber).HasMaxLength(50).IsRequired();
        builder.Property(e => e.Subject_En).HasMaxLength(500).IsRequired();
        builder.Property(e => e.Subject_Alt).HasMaxLength(500).IsRequired();
        builder.Property(e => e.FundYear).HasMaxLength(20);
        builder.Property(e => e.FundOwner).HasMaxLength(50);
        builder.Property(e => e.ReferenceNumber).HasMaxLength(200);
        builder.Property(e => e.EstimatedCost).HasColumnType("decimal(18,2)");
        builder.Property(e => e.ApprovedBudget).HasColumnType("decimal(18,2)");
        builder.Property(e => e.PreviousExpenditure).HasColumnType("decimal(18,2)");
        builder.Property(e => e.ProposedWorkCost).HasColumnType("decimal(18,2)");
        builder.Property(e => e.RemainingBalance).HasColumnType("decimal(18,2)");
        builder.Property(e => e.TechnicalApprovalNumber).HasMaxLength(100);
        builder.Property(e => e.TechnicalApprovalCost).HasColumnType("decimal(18,2)");
        builder.Property(e => e.CurrentStage).HasMaxLength(50).HasConversion<string>().IsRequired();
        builder.Property(e => e.PushBackCount).HasDefaultValue(0);

        // V1 Wizard Fields
        builder.Property(e => e.Reason_En).HasMaxLength(2000);
        builder.Property(e => e.Reason_Alt).HasMaxLength(2000);
        builder.Property(e => e.HomeId).HasMaxLength(100);
        builder.Property(e => e.AccountingNumber).HasMaxLength(100);
        builder.Property(e => e.PreviousExpenditureAmount).HasColumnType("decimal(18,2)");
        builder.Property(e => e.BalanceAmount).HasColumnType("decimal(18,2)");
        builder.Property(e => e.FirstApproverRole).HasMaxLength(50).HasConversion<string>();
        builder.Property(e => e.CompletedStep).HasDefaultValue(0);
        builder.Property(e => e.SubmitterDeclarationText_En).HasMaxLength(4000);
        builder.Property(e => e.SubmitterDeclarationText_Alt).HasMaxLength(4000);
        builder.Property(e => e.SubmitterRemarks_En).HasMaxLength(2000);
        builder.Property(e => e.SubmitterRemarks_Alt).HasMaxLength(2000);

        builder.HasIndex(e => e.ProposalNumber).IsUnique();
        builder.HasIndex(e => e.CurrentStage);
        builder.HasIndex(e => e.CreatedAt);
        builder.HasIndex(e => e.SubmittedById);
        builder.HasIndex(e => e.DepartmentId);

        builder.HasOne(e => e.Department)
            .WithMany(d => d.Proposals)
            .HasForeignKey(e => e.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.SubmittedBy)
            .WithMany(u => u.SubmittedProposals)
            .HasForeignKey(e => e.SubmittedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.SubmitterDesignation)
            .WithMany()
            .HasForeignKey(e => e.SubmitterDesignationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.FundType)
            .WithMany(f => f.Proposals)
            .HasForeignKey(e => e.FundTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Ward)
            .WithMany(w => w.Proposals)
            .HasForeignKey(e => e.WardId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.AccountHead)
            .WithMany(a => a.Proposals)
            .HasForeignKey(e => e.AccountHeadId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.AccountingOfficer)
            .WithMany()
            .HasForeignKey(e => e.AccountingOfficerId)
            .OnDelete(DeleteBehavior.ClientSetNull);

        builder.HasOne(e => e.ProcurementMethod)
            .WithMany(p => p.Proposals)
            .HasForeignKey(e => e.ProcurementMethodId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.TenderPublicationPeriod)
            .WithMany(t => t.Proposals)
            .HasForeignKey(e => e.TenderPublicationPeriodId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.CompetentAuthority)
            .WithMany()
            .HasForeignKey(e => e.CompetentAuthorityId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
