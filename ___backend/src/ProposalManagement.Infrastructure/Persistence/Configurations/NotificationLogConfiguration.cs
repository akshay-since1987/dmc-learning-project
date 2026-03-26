using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProposalManagement.Domain.Entities;

namespace ProposalManagement.Infrastructure.Persistence.Configurations;

public class NotificationLogConfiguration : IEntityTypeConfiguration<NotificationLog>
{
    public void Configure(EntityTypeBuilder<NotificationLog> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).UseIdentityColumn();
        builder.Property(e => e.MobileNumber).HasMaxLength(15).IsRequired();
        builder.Property(e => e.Channel).HasMaxLength(20).HasConversion<string>().IsRequired();
        builder.Property(e => e.TemplateName).HasMaxLength(100);
        builder.Property(e => e.Content).HasMaxLength(1000).IsRequired();
        builder.Property(e => e.Status).HasMaxLength(20).HasConversion<string>().IsRequired();
        builder.Property(e => e.ErrorMessage).HasMaxLength(500);

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(e => e.CreatedAt);
    }
}
