using AttendanceManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AttendanceManagement.Infrastructure.Persistence.Configurations;

internal sealed class GuardianStudentConfiguration : IEntityTypeConfiguration<GuardianStudent>
{
    public void Configure(EntityTypeBuilder<GuardianStudent> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("guardian_student");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.Relationship).HasConversion<string>().HasMaxLength(20).IsRequired();

        builder.HasIndex(x => new { x.GuardianId, x.StudentId })
            .IsUnique()
            .HasDatabaseName("ix_guardian_student_pair");
        builder.HasIndex(x => x.StudentId).HasDatabaseName("ix_guardian_student_student");

        // No máximo um contato principal ativo por aluno.
        builder.HasIndex(x => x.StudentId)
            .IsUnique()
            .HasFilter("is_primary AND active")
            .HasDatabaseName("ix_guardian_student_primary");

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
