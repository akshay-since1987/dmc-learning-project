using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProposalManagement.Domain.Entities;

namespace ProposalManagement.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.FullName_En).HasMaxLength(200).IsRequired();
        builder.Property(e => e.FullName_Alt).HasMaxLength(200).IsRequired();
        builder.Property(e => e.MobileNumber).HasMaxLength(15).IsRequired();
        builder.Property(e => e.Email).HasMaxLength(200);
        builder.Property(e => e.PasswordHash).HasMaxLength(500);
        builder.Property(e => e.Role).HasMaxLength(50).HasConversion<string>().IsRequired();
        builder.Property(e => e.SignaturePath).HasMaxLength(500);

        builder.HasIndex(e => e.MobileNumber).IsUnique();

        builder.HasOne(e => e.Department)
            .WithMany(d => d.Users)
            .HasForeignKey(e => e.DepartmentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.Designation)
            .WithMany(d => d.Users)
            .HasForeignKey(e => e.DesignationId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
