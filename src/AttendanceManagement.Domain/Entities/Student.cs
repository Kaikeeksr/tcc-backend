using AttendanceManagement.Domain.Abstractions;

namespace AttendanceManagement.Domain.Entities;

/// <summary>
/// O menor transportado. Guarda o mínimo de dado pessoal — contato mora no
/// guardian. Login é opcional.
/// </summary>
public sealed class Student : SoftDeletableEntity
{
    public const int NameMaxLength = 150;
    public const int GradeMaxLength = 20;

    private Student(Guid id, Guid transporterId, string name, DateOnly birthDate)
        : base(id)
    {
        TransporterId = transporterId;
        Name = name;
        BirthDate = birthDate;
    }

    private Student()
    {
    }

    /// <summary>Denormalizado (derivável via enrollment) para filtrar por tenant sem join.</summary>
    public Guid TransporterId { get; private set; }

    public Guid? UserAccountId { get; private set; }

    public string Name { get; private set; } = null!;

    public DateOnly BirthDate { get; private set; }

    /// <summary>Série/turma escolar, ex.: "3°A". Texto livre.</summary>
    public string? Grade { get; private set; }

    public Guid? SchoolId { get; private set; }

    public Transporter Transporter { get; private set; } = null!;

    public School? School { get; private set; }

    public UserAccount? UserAccount { get; private set; }

    public static Result<Student> Create(
        Guid transporterId,
        string? name,
        DateOnly birthDate,
        string? grade,
        Guid? schoolId)
    {
        if (transporterId == Guid.Empty)
        {
            return Result.Failure<Student>(Error.Validation("Student.TransporterRequired", "Transporter is required."));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<Student>(Error.Validation("Student.NameRequired", "Name is required."));
        }

        if (birthDate > DateOnly.FromDateTime(DateTime.UtcNow))
        {
            return Result.Failure<Student>(Error.Validation("Student.BirthDateInvalid", "The birth date cannot be in the future."));
        }

        var normalizedGrade = string.IsNullOrWhiteSpace(grade) ? null : grade.Trim();

        if (normalizedGrade?.Length > GradeMaxLength)
        {
            return Result.Failure<Student>(Error.Validation("Student.GradeTooLong", "The grade exceeds the maximum length."));
        }

        var student = new Student(Guid.CreateVersion7(), transporterId, name.Trim(), birthDate)
        {
            Grade = normalizedGrade,
        };

        if (schoolId is { } school && school != Guid.Empty)
        {
            student.SchoolId = school;
        }

        return Result.Success(student);
    }

    public void SetLogin(Guid? userAccountId)
    {
        UserAccountId = userAccountId == Guid.Empty ? null : userAccountId;
        Touch();
    }

    public void SetGrade(string? grade)
    {
        Grade = string.IsNullOrWhiteSpace(grade) ? null : grade.Trim();
        Touch();
    }

    public void TransferToSchool(Guid? schoolId)
    {
        SchoolId = schoolId == Guid.Empty ? null : schoolId;
        Touch();
    }

    public Result Update(string? name, DateOnly birthDate, string? grade)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure(Error.Validation("Student.NameRequired", "Name is required."));
        }

        if (birthDate > DateOnly.FromDateTime(DateTime.UtcNow))
        {
            return Result.Failure(Error.Validation("Student.BirthDateInvalid", "The birth date cannot be in the future."));
        }

        var normalizedGrade = string.IsNullOrWhiteSpace(grade) ? null : grade.Trim();

        if (normalizedGrade?.Length > GradeMaxLength)
        {
            return Result.Failure(Error.Validation("Student.GradeTooLong", "The grade exceeds the maximum length."));
        }

        Name = name.Trim();
        BirthDate = birthDate;
        Grade = normalizedGrade;
        Touch();
        return Result.Success();
    }
}
