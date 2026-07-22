using AttendanceManagement.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AttendanceManagement.Infrastructure.Persistence.Conventions;

/// <summary>
/// Mapeamento do value object <see cref="Address"/>, chamado dentro do
/// <c>OwnsOne</c> de cada dono para os limites das colunas `address_*` ficarem
/// em um lugar só.
/// </summary>
internal static class AddressMapping
{
    public static void Configure<TOwner>(OwnedNavigationBuilder<TOwner, Address> owned)
        where TOwner : class
    {
        ArgumentNullException.ThrowIfNull(owned);

        owned.Property(p => p.Street).HasMaxLength(200);
        owned.Property(p => p.Number).HasMaxLength(20);
        owned.Property(p => p.Complement).HasMaxLength(100);
        owned.Property(p => p.District).HasMaxLength(100);
        owned.Property(p => p.City).HasMaxLength(100);
        owned.Property(p => p.State).HasMaxLength(2);
        owned.Property(p => p.PostalCode).HasMaxLength(12);
    }
}
