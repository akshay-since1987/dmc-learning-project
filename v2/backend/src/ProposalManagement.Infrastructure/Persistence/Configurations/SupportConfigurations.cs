using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProposalManagement.Domain.Entities;

namespace ProposalManagement.Infrastructure.Persistence.Configurations;

public class ProposalApprovalConfiguration : IEntityTypeConfiguration<ProposalApproval>
{
    public void Configure(EntityTypeBuilder<ProposalApproval> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.StageRole).HasMaxLength(50).IsRequired();
        builder.Property(e => e.Action).HasMaxLength(30).IsRequired();
        builder.Property(e => e.ActorName_En).HasMaxLength(200);
        builder.Property(e => e.ActorName_Mr).HasMaxLength(200);
        builder.Property(e => e.ActorDesignation_En).HasMaxLength(200);
        builder.Property(e => e.ActorDesignation_Mr).HasMaxLength(200);
        builder.Property(e => e.DisclaimerText).IsRequired();
        builder.Property(e => e.Opinion_En).HasMaxLength(2000);
        builder.Property(e => e.Opinion_Mr).HasMaxLength(2000);
        builder.Property(e => e.SignaturePath).HasMaxLength(500);
        builder.Property(e => e.PushBackNote_En).HasMaxLength(2000);
        builder.Property(e => e.PushBackNote_Mr).HasMaxLength(2000);
        builder.Property(e => e.ConsolidatedPdfPath).HasMaxLength(500);
        builder.HasIndex(e => new { e.ProposalId, e.CreatedAt });

        builder.HasOne(e => e.Proposal).WithMany(p => p.Approvals).HasForeignKey(e => e.ProposalId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(e => e.Actor).WithMany().HasForeignKey(e => e.ActorId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class GeneratedPdfConfiguration : IEntityTypeConfiguration<GeneratedPdf>
{
    public void Configure(EntityTypeBuilder<GeneratedPdf> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.PdfType).HasMaxLength(50).IsRequired();
        builder.Property(e => e.StageRole).HasMaxLength(50);
        builder.Property(e => e.Title_En).HasMaxLength(300);
        builder.Property(e => e.Title_Mr).HasMaxLength(300);
        builder.Property(e => e.StoragePath).HasMaxLength(500).IsRequired();
        builder.HasIndex(e => new { e.ProposalId, e.PdfType });

        builder.HasOne(e => e.Proposal).WithMany(p => p.GeneratedPdfs).HasForeignKey(e => e.ProposalId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(e => e.GeneratedBy).WithMany().HasForeignKey(e => e.GeneratedById).OnDelete(DeleteBehavior.Restrict);
    }
}

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.Type).HasMaxLength(50).IsRequired();
        builder.Property(e => e.Title_En).HasMaxLength(300).IsRequired();
        builder.Property(e => e.Title_Mr).HasMaxLength(300);
        builder.Property(e => e.Message_En).HasMaxLength(1000).IsRequired();
        builder.Property(e => e.Message_Mr).HasMaxLength(1000);
        builder.HasIndex(e => new { e.UserId, e.IsRead, e.CreatedAt });
        builder.HasIndex(e => e.PalikaId);

        builder.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Palika).WithMany().HasForeignKey(e => e.PalikaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Proposal).WithMany(p => p.Notifications).HasForeignKey(e => e.ProposalId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class AuditTrailConfiguration : IEntityTypeConfiguration<AuditTrail>
{
    public void Configure(EntityTypeBuilder<AuditTrail> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.UserName).HasMaxLength(200);
        builder.Property(e => e.UserRole).HasMaxLength(50);
        builder.Property(e => e.IpAddress).HasMaxLength(45);
        builder.Property(e => e.UserAgent).HasMaxLength(500);
        builder.Property(e => e.Action).HasMaxLength(50).IsRequired();
        builder.Property(e => e.EntityType).HasMaxLength(100).IsRequired();
        builder.Property(e => e.EntityId).HasMaxLength(100);
        builder.Property(e => e.Description).HasMaxLength(1000);
        builder.Property(e => e.Module).HasMaxLength(50).IsRequired();
        builder.Property(e => e.Severity).HasMaxLength(20).IsRequired();
        builder.HasIndex(e => e.Timestamp);
        builder.HasIndex(e => new { e.EntityType, e.EntityId });
        builder.HasIndex(e => e.Module);
        builder.HasIndex(e => e.PalikaId);
    }
}
