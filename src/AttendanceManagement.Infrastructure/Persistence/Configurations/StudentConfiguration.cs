using AttendanceManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AttendanceManagement.Infrastructure.Persistence.Configurations;

internal sealed class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("student");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.Name).IsRequired().HasMaxLength(Student.NameMaxLength);
        builder.Property(x => x.BirthDate).IsRequired();
        // Série escolar ("3°A"), opcional.
        builder.Property(x => x.Grade).HasMaxLength(Student.GradeMaxLength);

        builder.HasIndex(x => x.TransporterId).HasDatabaseName("ix_student_transporter");
        // Monta a rota da volta: quais alunos buscar em cada escola.
        builder.HasIndex(x => x.SchoolId).HasDatabaseName("ix_student_school");

        builder.HasOne<Transporter>()
            .WithMany()
            .HasForeignKey(x => x.TransporterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<School>()
            .WithMany()
            .HasForeignKey(x => x.SchoolId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<UserAccount>()
            .WithMany()
            .HasForeignKey(x => x.UserAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(x => x.DeletedAtUtc == null);
    }
}
