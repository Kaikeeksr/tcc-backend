using AttendanceManagement.Application.Abstractions;
using AttendanceManagement.Domain.Abstractions;

namespace AttendanceManagement.Application.Transporters;

/// <summary>Caso de uso "ver minha equipe de transporte". Traduz o <c>null</c> do repositório em 404.</summary>
public sealed class TransportTeamService(ITransporterRepository repository)
{
    public async Task<Result<TransportTeam>> GetByUserAccountAsync(
        Guid userAccountId,
        CancellationToken cancellationToken = default)
    {
        if (userAccountId == Guid.Empty)
        {
            return Result.Failure<TransportTeam>(
                Error.Validation("TransportTeam.UserRequired", "The login account is required."));
        }

        var team = await repository.GetTeamByUserAccountAsync(userAccountId, cancellationToken);

        return team is null
            ? Result.Failure<TransportTeam>(
                Error.NotFound("TransportTeam.NotFound", "No transporter linked to this account."))
            : Result.Success(team);
    }
}
