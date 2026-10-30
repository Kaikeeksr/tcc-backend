using AttendanceManagement.Application.GuardianConsents;
using AttendanceManagement.Application.Tests.Fakes;
using Xunit;

namespace AttendanceManagement.Application.Tests.GuardianConsents;

public class GuardianConsentServiceTests
{
    private readonly FakeGuardianConsentRepository consents = new();
    private readonly FakeGuardianStudentRepository guardianStudents = new();
    private readonly FakeEventLogRepository eventLog = new();
    private readonly GuardianConsentService service;

    public GuardianConsentServiceTests()
    {
        service = new GuardianConsentService(consents, guardianStudents, eventLog, new InMemoryUnitOfWork());
    }

    [Fact]
    public async Task GrantAsync_without_an_active_link_is_unauthorized()
    {
        var transporterId = Guid.NewGuid();
        var guardianId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        // Nenhum vínculo ativo registrado em guardianStudents.ActivePairs.

        var result = await service.GrantAsync(
            transporterId, guardianId, studentId, Guid.NewGuid(), new GrantGuardianConsentRequest("1.0"));

        Assert.True(result.IsFailure);
        Assert.Equal("GuardianConsent.NotLinked", result.Error.Code);
    }

    [Fact]
    public async Task GrantAsync_with_an_active_link_grants_and_logs_event()
    {
        var transporterId = Guid.NewGuid();
        var guardianId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        guardianStudents.ActivePairs.Add((guardianId, studentId));
        consents.ChildrenByGuardian[guardianId] = [(studentId, "Aluno Teste")];

        var result = await service.GrantAsync(
            transporterId, guardianId, studentId, Guid.NewGuid(), new GrantGuardianConsentRequest("1.0"));

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.Active);
        Assert.Equal("1.0", result.Value.TermsVersion);
        Assert.Contains(eventLog.Entries, e => e.EventType == "GuardianConsent.Granted");
    }

    [Fact]
    public async Task RevokeAsync_then_grant_again_creates_a_new_row_and_preserves_history()
    {
        var transporterId = Guid.NewGuid();
        var guardianId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        guardianStudents.ActivePairs.Add((guardianId, studentId));
        consents.ChildrenByGuardian[guardianId] = [(studentId, "Aluno Teste")];

        await service.GrantAsync(transporterId, guardianId, studentId, Guid.NewGuid(), new GrantGuardianConsentRequest("1.0"));
        var revoke = await service.RevokeAsync(transporterId, guardianId, studentId, Guid.NewGuid());
        Assert.True(revoke.IsSuccess);

        var regrant = await service.GrantAsync(transporterId, guardianId, studentId, Guid.NewGuid(), new GrantGuardianConsentRequest("1.1"));

        Assert.True(regrant.IsSuccess);
        Assert.Equal(2, consents.Consents.Count);
        Assert.Single(consents.Consents, c => c.RevokedAtUtc is not null);
        Assert.Single(consents.Consents, c => c.RevokedAtUtc is null && c.TermsVersion == "1.1");
        Assert.Contains(eventLog.Entries, e => e.EventType == "GuardianConsent.Revoked");
    }

    [Fact]
    public async Task RevokeAsync_twice_fails()
    {
        var transporterId = Guid.NewGuid();
        var guardianId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        guardianStudents.ActivePairs.Add((guardianId, studentId));
        consents.ChildrenByGuardian[guardianId] = [(studentId, "Aluno Teste")];
        await service.GrantAsync(transporterId, guardianId, studentId, Guid.NewGuid(), new GrantGuardianConsentRequest("1.0"));
        await service.RevokeAsync(transporterId, guardianId, studentId, Guid.NewGuid());

        var result = await service.RevokeAsync(transporterId, guardianId, studentId, Guid.NewGuid());

        Assert.True(result.IsFailure);
        Assert.Equal("GuardianConsent.NotActive", result.Error.Code);
    }
}
