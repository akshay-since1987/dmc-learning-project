using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProposalManagement.Domain.Entities;

namespace ProposalManagement.Infrastructure.Persistence.Configurations;

public class FundTypeConfiguration : IEntityTypeConfiguration<FundType>
{
    public void Configure(EntityTypeBuilder<FundType> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name_En).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Name_Alt).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Code).HasMaxLength(50);
        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
