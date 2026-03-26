using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProposalManagement.Domain.Entities;

namespace ProposalManagement.Infrastructure.Persistence.Configurations;

public class ProposalSignatureConfiguration : IEntityTypeConfiguration<ProposalSignature>
{
    public void Configure(EntityTypeBuilder<ProposalSignature> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.PositionX).HasColumnType("decimal(10,4)");
        builder.Property(e => e.PositionY).HasColumnType("decimal(10,4)");
        builder.Property(e => e.Width).HasColumnType("decimal(10,4)");
        builder.Property(e => e.Height).HasColumnType("decimal(10,4)");
        builder.Property(e => e.Rotation).HasColumnType("decimal(10,4)");
        builder.Property(e => e.GeneratedPdfPath).HasMaxLength(500).IsRequired();

        builder.HasIndex(e => e.ProposalId);
        builder.HasIndex(e => e.StageHistoryId);

        builder.HasOne(e => e.Proposal)
            .WithMany(p => p.Signatures)
            .HasForeignKey(e => e.ProposalId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.StageHistory)
            .WithMany()
            .HasForeignKey(e => e.StageHistoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.SignedBy)
            .WithMany()
            .HasForeignKey(e => e.SignedById)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
