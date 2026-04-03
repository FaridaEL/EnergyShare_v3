using EnergyShare_v3.Domain.Enums;

namespace EnergyShare_v3.Application.Features.Users
{       /*DTO pour afficher un user dans une liste.
         * Les DTOs (Data Transfer Objects) sont des record : immutables, avec comparaison par valeur et deconstruction automatique.
         * Ils servent a transporter des donnees entre les couches sans exposer les entites du domaine..*/
    public record UserSummaryDto(
      Guid Id,
      string? FirstName,
      string? LastName,
      string Email, // DTO : on utilise string pour simplifier le transport (pas de ValueObject ici)
      UserRole Role,
      UserType UserType,
      DateTime CreatedAt
    );
    /// <summary>
    /// DTO pour afficher le detail d'un user et de ses partage.
    /// </summary>
    public record UserDetailsDto(
        Guid Id,
        string? FirstName,
        DateTime CreatedAt
        //IReadOnlyList<MemberDto> Members
    );


}
