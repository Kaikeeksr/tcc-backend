using AttendanceManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AttendanceManagement.Infrastructure.Persistence.Configurations;

internal sealed class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
{
    public void Configure(EntityTypeBuilder<Enrollment> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("enrollment");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.HasIndex(x => x.TransportGroupId).HasDatabaseName("ix_enrollment_transport_group");
        builder.HasIndex(x => x.StudentId).HasDatabaseName("ix_enrollment_student");

        // Índice parcial: com `active` no filtro (não nas colunas), o Postgres
        // resolve o count() de alunos por grupo só pelo índice, sem visitar a heap.
        builder.HasIndex(x => x.TransportGroupId, "ix_enrollment_active_group")
            .HasFilter("active")
            .HasDatabaseName("ix_enrollment_active_group");

        builder.HasIndex(x => new { x.StudentId, x.TransportGroupId })
            .IsUnique()
            .HasFilter("active")
            .HasDatabaseName("ix_enrollment_active");

        builder.HasOne(x => x.Student)
            .WithMany()
            .HasForeignKey(x => x.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.TransportGroup)
            .WithMany()
            .HasForeignKey(x => x.TransportGroupId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
