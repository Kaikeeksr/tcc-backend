using AttendanceManagement.Domain.Entities;
using Xunit;

namespace AttendanceManagement.Domain.Tests.Entities;

public class GuardianConsentTests
{
    [Fact]
    public void Create_with_valid_data_succeeds_and_starts_active()
    {
        var result = GuardianConsent.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "1.0");

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.IsActive);
        Assert.Null(result.Value.RevokedAtUtc);
        Assert.Equal("1.0", result.Value.TermsVersion);
    }

    [Fact]
    public void Create_requires_guardian()
    {
        var result = GuardianConsent.Create(Guid.NewGuid(), Guid.Empty, Guid.NewGuid(), "1.0");

        Assert.True(result.IsFailure);
        Assert.Equal("GuardianConsent.GuardianRequired", result.Error.Code);
    }

    [Fact]
    public void Create_requires_student()
    {
        var result = GuardianConsent.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.Empty, "1.0");

        Assert.True(result.IsFailure);
        Assert.Equal("GuardianConsent.StudentRequired", result.Error.Code);
    }

    [Fact]
    public void Create_requires_terms_version()
    {
        var result = GuardianConsent.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), " ");

        Assert.True(result.IsFailure);
        Assert.Equal("GuardianConsent.TermsVersionRequired", result.Error.Code);
    }

    [Fact]
    public void Revoke_deactivates_consent()
    {
        var consent = GuardianConsent.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "1.0").Value;

        var result = consent.Revoke();

        Assert.True(result.IsSuccess);
        Assert.False(consent.IsActive);
        Assert.NotNull(consent.RevokedAtUtc);
    }

    [Fact]
    public void Revoke_twice_fails()
    {
        var consent = GuardianConsent.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "1.0").Value;
        consent.Revoke();

        var result = consent.Revoke();

        Assert.True(result.IsFailure);
        Assert.Equal("GuardianConsent.AlreadyRevoked", result.Error.Code);
    }
}
