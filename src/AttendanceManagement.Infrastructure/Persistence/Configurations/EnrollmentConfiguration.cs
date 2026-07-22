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
        // Evita duas matrículas ativas do mesmo aluno no mesmo grupo.
        builder.HasIndex(x => new { x.StudentId, x.TransportGroupId })
            .IsUnique()
            .HasFilter("active")
            .HasDatabaseName("ix_enrollment_active");

        builder.HasOne<Student>()
            .WithMany()
            .HasForeignKey(x => x.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<TransportGroup>()
            .WithMany()
            .HasForeignKey(x => x.TransportGroupId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
