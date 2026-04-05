using System.Data.Common;

namespace EnergyShare_v3.Application.Interfaces
{        /* Le CDC prévoit EF Core pour l’écriture et Dapper pour certaines lectures rapides
            Donc cette interface a du sens pour la suite pour faire :
            - dashboards
            - statistiques
            - exports
            - lectures optimisées
          */

    public interface IDbConnectionFactory
    {
        DbConnection GetConnection();
    }
}
