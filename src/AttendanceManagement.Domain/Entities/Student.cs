using AttendanceManagement.Domain.Abstractions;

namespace AttendanceManagement.Domain.Entities;

/// <summary>
/// O menor transportado. Guarda o mínimo de dado pessoal — contato mora no
/// guardian.
///
/// Login é opcional. A regra "todo aluno tem ≥1 guardian" não é constraint de
/// banco (exigiria trigger); vale na camada de aplicação.
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

    /// <summary>Série/turma escolar, ex.: "3°A". Texto livre — cada escola nomeia à sua maneira.</summary>
    public string? Grade { get; private set; }

    /// <summary>Escola atual. Muda ao longo do tempo; a chamada guarda snapshot.</summary>
    public Guid? SchoolId { get; private set; }

    public static Result<Student> Create(
        Guid transporterId,
        string? name,
        DateOnly birthDate,
        string? grade,
        Guid? schoolId)
    {
        if (transporterId == Guid.Empty)
        {
            return Result.Failure<Student>(Error.Validation("Student.TransporterRequired", "O transportador é obrigatório."));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<Student>(Error.Validation("Student.NameRequired", "O nome é obrigatório."));
        }

        if (birthDate > DateOnly.FromDateTime(DateTime.UtcNow))
        {
            return Result.Failure<Student>(Error.Validation("Student.BirthDateInvalid", "A data de nascimento não pode ser futura."));
        }

        var normalizedGrade = string.IsNullOrWhiteSpace(grade) ? null : grade.Trim();

        if (normalizedGrade?.Length > GradeMaxLength)
        {
            return Result.Failure<Student>(Error.Validation("Student.GradeTooLong", "A turma excede o tamanho máximo."));
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
}
