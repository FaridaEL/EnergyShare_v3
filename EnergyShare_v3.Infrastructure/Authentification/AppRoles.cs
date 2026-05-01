using System;
using System.Collections.Generic;
using System.Text;

namespace EnergyShare_v3.Infrastructure.Authentification
{
    public static class AppRoles
    {
        public const string Utilisateur = "Utilisateur";
        public const string OrganismePublic = "OrganismePublic";
        public const string Administrateur = "Administrateur";

        public static readonly string[] All =
        [
            Utilisateur,
        OrganismePublic,
        Administrateur
        ];
    }
}
