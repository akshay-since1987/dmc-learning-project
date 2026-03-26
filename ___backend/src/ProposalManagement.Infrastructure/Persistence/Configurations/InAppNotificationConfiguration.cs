using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProposalManagement.Domain.Entities;

namespace ProposalManagement.Infrastructure.Persistence.Configurations;

public class InAppNotificationConfiguration : IEntityTypeConfiguration<InAppNotification>
{
    public void Configure(EntityTypeBuilder<InAppNotification> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).UseIdentityColumn();
        builder.Property(e => e.Title_En).HasMaxLength(300).IsRequired();
        builder.Property(e => e.Title_Alt).HasMaxLength(300).IsRequired();
        builder.Property(e => e.Message_En).HasMaxLength(1000).IsRequired();
        builder.Property(e => e.Message_Alt).HasMaxLength(1000).IsRequired();
        builder.Property(e => e.LinkUrl).HasMaxLength(500);

        builder.HasIndex(e => new { e.UserId, e.IsRead });
        builder.HasIndex(e => e.CreatedAt);

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
