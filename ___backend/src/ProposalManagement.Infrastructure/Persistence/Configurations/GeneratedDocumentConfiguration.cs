using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProposalManagement.Domain.Entities;

namespace ProposalManagement.Infrastructure.Persistence.Configurations;

public class GeneratedDocumentConfiguration : IEntityTypeConfiguration<GeneratedDocument>
{
    public void Configure(EntityTypeBuilder<GeneratedDocument> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.DocumentKind).HasMaxLength(50).HasConversion<string>().IsRequired();
        builder.Property(e => e.Title_En).HasMaxLength(300).IsRequired();
        builder.Property(e => e.Title_Alt).HasMaxLength(300).IsRequired();
        builder.Property(e => e.StoragePath).HasMaxLength(500).IsRequired();

        builder.HasOne(e => e.Proposal)
            .WithMany(p => p.GeneratedDocuments)
            .HasForeignKey(e => e.ProposalId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.GeneratedBy)
            .WithMany()
            .HasForeignKey(e => e.GeneratedById)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
