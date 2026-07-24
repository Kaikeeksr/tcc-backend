using AttendanceManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AttendanceManagement.Infrastructure.Persistence.Configurations;

internal sealed class TransporterConfiguration : IEntityTypeConfiguration<Transporter>
{
    public void Configure(EntityTypeBuilder<Transporter> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("transporter");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.Name).IsRequired().HasMaxLength(Transporter.NameMaxLength);
        builder.Property(x => x.DocumentType).HasConversion<string>().HasMaxLength(10).IsRequired();
        builder.Property(x => x.DocumentNumber).IsRequired().HasMaxLength(20);
        builder.Property(x => x.LicenseNumber).HasMaxLength(20);
        builder.Property(x => x.LicenseCategory).HasMaxLength(5);
        builder.Property(x => x.BusinessPhone).HasMaxLength(30);

        builder.HasIndex(x => x.UserAccountId).IsUnique().HasDatabaseName("ix_transporter_user_account");
        builder.HasIndex(x => x.DocumentNumber).IsUnique().HasDatabaseName("ix_transporter_document");

        builder.HasOne(x => x.UserAccount)
            .WithMany()
            .HasForeignKey(x => x.UserAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(x => x.DeletedAtUtc == null);
    }
}
