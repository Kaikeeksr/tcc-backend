using AttendanceManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AttendanceManagement.Infrastructure.Persistence.Configurations;

internal sealed class GuardianConsentConfiguration : IEntityTypeConfiguration<GuardianConsent>
{
    public void Configure(EntityTypeBuilder<GuardianConsent> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("guardian_consent");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.TermsVersion).IsRequired().HasMaxLength(20);

        builder.Ignore(x => x.IsActive);

        // No máximo um consentimento ativo (não revogado) por par responsável/aluno.
        builder.HasIndex(x => new { x.GuardianId, x.StudentId })
            .IsUnique()
            .HasFilter("revoked_at_utc IS NULL")
            .HasDatabaseName("ix_guardian_consent_active");

        builder.HasOne(x => x.Guardian)
            .WithMany()
            .HasForeignKey(x => x.GuardianId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Student)
            .WithMany()
            .HasForeignKey(x => x.StudentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
