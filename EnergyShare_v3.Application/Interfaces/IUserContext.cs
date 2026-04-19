using System;
using System.Collections.Generic;
using System.Text;

namespace EnergyShare_v3.Application.Interfaces
{          /*
            IUserContext expose les informations de l'utilisateur courant
            sous forme d'abstraction applicative.

            Cela évite de dépendre directement de HttpContext dans les handlers
            ou services métier.
        */
    public interface IUserContext
    {
        Guid? UserId { get; }
        string? Email { get; }
        string? UserName { get; }
        bool IsAuthenticated { get; }
        IReadOnlyList<string> Roles { get; }
        Guid? OrganismePublicId { get; }

        bool IsInRole(string role);
    }
}
