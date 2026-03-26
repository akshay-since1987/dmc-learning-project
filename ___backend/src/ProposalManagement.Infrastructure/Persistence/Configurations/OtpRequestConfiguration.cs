using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProposalManagement.Domain.Entities;

namespace ProposalManagement.Infrastructure.Persistence.Configurations;

public class OtpRequestConfiguration : IEntityTypeConfiguration<OtpRequest>
{
    public void Configure(EntityTypeBuilder<OtpRequest> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).UseIdentityColumn();
        builder.Property(e => e.MobileNumber).HasMaxLength(15).IsRequired();
        builder.Property(e => e.OtpHash).HasMaxLength(500).IsRequired();
        builder.Property(e => e.Purpose).HasMaxLength(50).HasConversion<string>().IsRequired();

        builder.HasIndex(e => new { e.MobileNumber, e.CreatedAt });
    }
}
