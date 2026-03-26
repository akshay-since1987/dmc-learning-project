using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProposalManagement.Domain.Entities;

namespace ProposalManagement.Infrastructure.Persistence.Configurations;

public class FieldVisitConfiguration : IEntityTypeConfiguration<FieldVisit>
{
    public void Configure(EntityTypeBuilder<FieldVisit> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasQueryFilter(e => !e.IsDeleted);

        builder.Property(e => e.ProblemDescription_En).HasMaxLength(2000);
        builder.Property(e => e.ProblemDescription_Mr).HasMaxLength(2000);
        builder.Property(e => e.Measurements_En).HasMaxLength(1000);
        builder.Property(e => e.Measurements_Mr).HasMaxLength(1000);
        builder.Property(e => e.GpsLatitude).HasColumnType("decimal(10,7)");
        builder.Property(e => e.GpsLongitude).HasColumnType("decimal(10,7)");
        builder.Property(e => e.Remark_En).HasMaxLength(2000);
        builder.Property(e => e.Remark_Mr).HasMaxLength(2000);
        builder.Property(e => e.Recommendation_En).HasMaxLength(2000);
        builder.Property(e => e.Recommendation_Mr).HasMaxLength(2000);
        builder.Property(e => e.UploadedPdfPath).HasMaxLength(500);
        builder.Property(e => e.SignaturePath).HasMaxLength(500);
        builder.Property(e => e.Status).HasMaxLength(30).IsRequired();
        builder.HasIndex(e => new { e.ProposalId, e.VisitNumber });

        builder.HasOne(e => e.Proposal).WithMany(p => p.FieldVisits).HasForeignKey(e => e.ProposalId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(e => e.AssignedTo).WithMany().HasForeignKey(e => e.AssignedToId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.AssignedBy).WithMany().HasForeignKey(e => e.AssignedById).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.InspectionBy).WithMany().HasForeignKey(e => e.InspectionById).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.SiteCondition).WithMany().HasForeignKey(e => e.SiteConditionId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class FieldVisitPhotoConfiguration : IEntityTypeConfiguration<FieldVisitPhoto>
{
    public void Configure(EntityTypeBuilder<FieldVisitPhoto> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.FileName).HasMaxLength(300).IsRequired();
        builder.Property(e => e.ContentType).HasMaxLength(100).IsRequired();
        builder.Property(e => e.StoragePath).HasMaxLength(500).IsRequired();
        builder.Property(e => e.Caption).HasMaxLength(500);

        builder.HasOne(e => e.FieldVisit).WithMany(fv => fv.Photos).HasForeignKey(e => e.FieldVisitId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class EstimateConfiguration : IEntityTypeConfiguration<Estimate>
{
    public void Configure(EntityTypeBuilder<Estimate> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasQueryFilter(e => !e.IsDeleted);

        builder.Property(e => e.EstimatePdfPath).HasMaxLength(500);
        builder.Property(e => e.EstimatedCost).HasColumnType("decimal(18,2)");
        builder.Property(e => e.PreparedSignaturePath).HasMaxLength(500);
        builder.Property(e => e.SentToRole).HasMaxLength(50);
        builder.Property(e => e.ApproverSignaturePath).HasMaxLength(500);
        builder.Property(e => e.ApproverOpinion_En).HasMaxLength(2000);
        builder.Property(e => e.ApproverOpinion_Mr).HasMaxLength(2000);
        builder.Property(e => e.Status).HasMaxLength(30).IsRequired();
        builder.Property(e => e.ReturnQueryNote_En).HasMaxLength(2000);
        builder.Property(e => e.ReturnQueryNote_Mr).HasMaxLength(2000);
        builder.HasIndex(e => e.ProposalId).IsUnique();

        builder.HasOne(e => e.Proposal).WithOne(p => p.Estimate).HasForeignKey<Estimate>(e => e.ProposalId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(e => e.PreparedBy).WithMany().HasForeignKey(e => e.PreparedById).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.SentTo).WithMany().HasForeignKey(e => e.SentToId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.ApprovedBy).WithMany().HasForeignKey(e => e.ApprovedById).OnDelete(DeleteBehavior.Restrict);
    }
}

public class TechnicalSanctionConfiguration : IEntityTypeConfiguration<TechnicalSanction>
{
    public void Configure(EntityTypeBuilder<TechnicalSanction> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasQueryFilter(e => !e.IsDeleted);

        builder.Property(e => e.TsNumber).HasMaxLength(100);
        builder.Property(e => e.TsAmount).HasColumnType("decimal(18,2)");
        builder.Property(e => e.Description_En).HasMaxLength(2000);
        builder.Property(e => e.Description_Mr).HasMaxLength(2000);
        builder.Property(e => e.TsPdfPath).HasMaxLength(500);
        builder.Property(e => e.OutsideApprovalLetterPath).HasMaxLength(500);
        builder.Property(e => e.SanctionedByName).HasMaxLength(200);
        builder.Property(e => e.SanctionedByDept).HasMaxLength(200);
        builder.Property(e => e.SanctionedByDesignation).HasMaxLength(200);
        builder.Property(e => e.SignerSignaturePath).HasMaxLength(500);
        builder.Property(e => e.Status).HasMaxLength(30).IsRequired();
        builder.HasIndex(e => e.ProposalId).IsUnique();

        builder.HasOne(e => e.Proposal).WithOne(p => p.TechnicalSanction).HasForeignKey<TechnicalSanction>(e => e.ProposalId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(e => e.PreparedBy).WithMany().HasForeignKey(e => e.PreparedById).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.SignedBy).WithMany().HasForeignKey(e => e.SignedById).OnDelete(DeleteBehavior.Restrict);
    }
}

public class PramaDetailConfiguration : IEntityTypeConfiguration<PramaDetail>
{
    public void Configure(EntityTypeBuilder<PramaDetail> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasQueryFilter(e => !e.IsDeleted);

        builder.Property(e => e.FundApprovalYear).HasMaxLength(20);
        builder.Property(e => e.DeptUserName_En).HasMaxLength(200);
        builder.Property(e => e.DeptUserName_Mr).HasMaxLength(200);
        builder.HasIndex(e => e.ProposalId).IsUnique();

        builder.HasOne(e => e.Proposal).WithOne(p => p.PramaDetail).HasForeignKey<PramaDetail>(e => e.ProposalId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(e => e.FundType).WithMany().HasForeignKey(e => e.FundTypeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.BudgetHead).WithMany().HasForeignKey(e => e.BudgetHeadId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class BudgetDetailConfiguration : IEntityTypeConfiguration<BudgetDetail>
{
    public void Configure(EntityTypeBuilder<BudgetDetail> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasQueryFilter(e => !e.IsDeleted);

        builder.Property(e => e.AllocatedFund).HasColumnType("decimal(18,2)");
        builder.Property(e => e.CurrentAvailableFund).HasColumnType("decimal(18,2)");
        builder.Property(e => e.OldExpenditure).HasColumnType("decimal(18,2)");
        builder.Property(e => e.EstimatedCost).HasColumnType("decimal(18,2)");
        builder.Property(e => e.BalanceAmount).HasColumnType("decimal(18,2)");
        builder.Property(e => e.AccountSerialNo).HasMaxLength(100);
        builder.Property(e => e.ComplianceNotes_En).HasMaxLength(2000);
        builder.Property(e => e.ComplianceNotes_Mr).HasMaxLength(2000);
        builder.Property(e => e.DeterminedApprovalSlab).HasMaxLength(50);
        builder.Property(e => e.FinalAuthorityRole).HasMaxLength(50);
        builder.HasIndex(e => e.ProposalId).IsUnique();

        builder.HasOne(e => e.Proposal).WithOne(p => p.BudgetDetail).HasForeignKey<BudgetDetail>(e => e.ProposalId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(e => e.WorkExecutionMethod).WithMany().HasForeignKey(e => e.WorkExecutionMethodId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.BudgetHead).WithMany().HasForeignKey(e => e.BudgetHeadId).OnDelete(DeleteBehavior.Restrict);
    }
}
