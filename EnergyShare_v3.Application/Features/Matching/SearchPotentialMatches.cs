using System;
using System.Collections.Generic;
using System.Text;

namespace EnergyShare_v3.Application.Features.Matching
{      /* Todo : 
        * Le calcul de distance
         la recherche des profils compatibles
         la règle “disponible / non disponible”*/
    public record SearchPotentialMatchesQuery (Guid SourcePointAccessId);
    public class SearchPotentialMatchesHandler
    {
    }
}
