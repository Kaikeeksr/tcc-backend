namespace AttendanceManagement.Domain.Abstractions;

/// <summary>Natureza do erro. A camada de API traduz cada valor num status HTTP.</summary>
public enum ErrorType
{
    Failure = 0,
    Validation = 1,
    NotFound = 2,
    Conflict = 3,
    Unauthorized = 4,
}
