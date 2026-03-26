using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProposalManagement.Domain.Entities;

namespace ProposalManagement.Infrastructure.Persistence.Configurations;

public class ProposalConfiguration : IEntityTypeConfiguration<Proposal>
{
    public void Configure(EntityTypeBuilder<Proposal> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasQueryFilter(e => !e.IsDeleted);

        builder.Property(e => e.ProposalNumber).HasMaxLength(50).IsRequired();
        builder.HasIndex(e => e.ProposalNumber).IsUnique();

        builder.Property(e => e.WorkTitle_En).HasMaxLength(500).IsRequired();
        builder.Property(e => e.WorkTitle_Mr).HasMaxLength(500);
        builder.Property(e => e.WorkDescription_En).IsRequired();
        builder.Property(e => e.WorkDescription_Mr);
        builder.Property(e => e.LocationAddress_En).HasMaxLength(500);
        builder.Property(e => e.LocationAddress_Mr).HasMaxLength(500);
        builder.Property(e => e.LocationMapPath).HasMaxLength(500);
        builder.Property(e => e.Area).HasMaxLength(200);

        builder.Property(e => e.RequestorName).HasMaxLength(200);
        builder.Property(e => e.RequestorMobile).HasMaxLength(15);
        builder.Property(e => e.RequestorAddress).HasMaxLength(500);
        builder.Property(e => e.RequestorDesignation).HasMaxLength(200);
        builder.Property(e => e.RequestorOrganisation).HasMaxLength(300);
        builder.Property(e => e.Priority).HasMaxLength(20);

        builder.Property(e => e.CurrentStage).HasMaxLength(50).IsRequired();
        builder.HasIndex(e => new { e.PalikaId, e.CurrentStage });
        builder.HasIndex(e => e.CurrentOwnerId);
        builder.Property(e => e.ParkedAtStage).HasMaxLength(50);

        // FKs
        builder.HasOne(e => e.Palika).WithMany(p => p.Proposals).HasForeignKey(e => e.PalikaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Department).WithMany().HasForeignKey(e => e.DepartmentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.DeptWorkCategory).WithMany().HasForeignKey(e => e.DeptWorkCategoryId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.CreatedBy).WithMany().HasForeignKey(e => e.CreatedById).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Zone).WithMany().HasForeignKey(e => e.ZoneId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Prabhag).WithMany().HasForeignKey(e => e.PrabhagId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.RequestSource).WithMany().HasForeignKey(e => e.RequestSourceId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.CurrentOwner).WithMany().HasForeignKey(e => e.CurrentOwnerId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class ProposalDocumentConfiguration : IEntityTypeConfiguration<ProposalDocument>
{
    public void Configure(EntityTypeBuilder<ProposalDocument> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasQueryFilter(e => !e.IsDeleted);

        builder.Property(e => e.DocumentType).HasMaxLength(50).IsRequired();
        builder.Property(e => e.DocName).HasMaxLength(300);
        builder.Property(e => e.FileName).HasMaxLength(300).IsRequired();
        builder.Property(e => e.ContentType).HasMaxLength(100).IsRequired();
        builder.Property(e => e.StoragePath).HasMaxLength(500).IsRequired();
        builder.HasIndex(e => new { e.ProposalId, e.TabNumber });

        builder.HasOne(e => e.Proposal).WithMany(p => p.Documents).HasForeignKey(e => e.ProposalId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(e => e.UploadedBy).WithMany().HasForeignKey(e => e.UploadedById).OnDelete(DeleteBehavior.Restrict);
    }
}
