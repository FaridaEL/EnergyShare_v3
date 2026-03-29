using Ardalis.Result;

namespace EnergyShare_v3.Domain.Entities.Matchs.Match
{
    public static class MatchErros
    {
         /*Result Pattern -> quand l'utiliser ? 
          Si la méthode peut échouer  pour une raison métier attendue --> Result
         Si modification simpel sans cas d'échec --> void
         si création avec règle métier Create(...) avec Result<T>*/
        /*Règle de gestion  à transformer 
        public void VerifierCohérence()
        {
            if (PointAccessVendeurId == PointAccessAcheteurId)
                throw new InvalidOperationException("Un point d'accès ne peut pas être mis en relation avec lui-même.");

            if (DistanceCalculee < 0)
                throw new InvalidOperationException("La distance calculée ne peut pas être négative.");
        }*/

        public static Result SameAccessPoint(Guid vendeurId, Guid acheteurId) =>
        Result.Invalid(new ValidationError(
             $"{vendeurId}-{acheteurId}",
            "Un point d'accès ne peut pas être mis en relation avec lui-même",
            "Matchs.SameAccessPoint",
            ValidationSeverity.Error));

        public static Result DistanceNegative(decimal distance) =>
            Result.Invalid(new ValidationError(
                distance.ToString(),
                "La distance calculée ne peut pas être négative",
                "Matchs.DistanceNegative",
                ValidationSeverity.Error));


    }
}
