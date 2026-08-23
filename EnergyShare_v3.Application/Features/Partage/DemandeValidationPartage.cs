using Ardalis.Result;
using EnergyShare_v3.Application.Interfaces;
using EnergyShare_v3.Domain.Entities.Partages;
using EnergyShare_v3.Domain.Enums;
using Mediator;
using Microsoft.EntityFrameworkCore;

//____________________________________________
//VALIDATION INITIALE DU PARTAGE PAR LE GRD 
//____________________________________________
namespace EnergyShare_v3.Application.Features.Partage
{     

    // Commande envoyée quand le vendeur clique sur "Demander validation GRD".
    // Elle contient uniquement l'Id du partage, car l'utilisateur connecté est récupéré via IUserContext.
    public record DemandeValidationPartage(Guid PartageId)
        : ICommand<Result<DemandeValidationPartageDto>>;

    public class DemandeValidationPartageHandler(
        IApplicationDbContext context,
        IUserContext userContext)
        : ICommandHandler<DemandeValidationPartage, Result<DemandeValidationPartageDto>>
    {
        public async ValueTask<Result<DemandeValidationPartageDto>> Handle(
            DemandeValidationPartage command,
            CancellationToken cancellationToken)
        {
            // 1. Vérifie qu’un utilisateur est connecté.
            if (!userContext.IsAuthenticated || userContext.UserId is null)
                return Result<DemandeValidationPartageDto>.Unauthorized();

            var userId = userContext.UserId.Value;

            // 2. Charge le partage avec les données nécessaires :
            // - Membres + PointAccess : pour vérifier les participants et construire le détail du partage envoyé au GRD.
            // - DemandesGrd : pour éviter de créer plusieurs demandes de validation en attente.
            var partage = await context.Partages
                .Include(p => p.Membres)
                    .ThenInclude(m => m.PointAccess)
                .Include(p => p.DemandesGrd)
                .FirstOrDefaultAsync(p => p.Id == command.PartageId, cancellationToken);

            if (partage is null)
                return Result<DemandeValidationPartageDto>.NotFound();

            // 3. Seul le vendeur / créateur du partage peut demander la validation.
            if (partage.VendeurId != userId)
                return Result<DemandeValidationPartageDto>.Forbidden();

            // 4. Vérifie que le partage est complet :
            // - PairToPair : --> 2 membres actifs.
            // - Même bâtiment : min 2 membres actifs.
            var validationMembres = partage.VerifierNombreMembres();

            if (!validationMembres.IsSuccess)
                return Result<DemandeValidationPartageDto>.Invalid(validationMembres.ValidationErrors);

            // 5. Pour un partage "Même bâtiment", le périmètre est par défaut A .
            // On le définit automatiquement si pas encore fait.
            if (partage.EnergieType == PartageEnergieType.MemeBatiment &&
                partage.Perimetre is null)
            {
                var perimetreResult = partage.DefinirPerimetre(PerimetreType.A);

                if (!perimetreResult.IsSuccess)
                    return Result<DemandeValidationPartageDto>.Invalid(perimetreResult.ValidationErrors);
            }

            // 6. Pour un partage pair-à-pair, il faut que le périmètre soit déjà confirmé.
            // TODO :
            // À l'avenir, on pourra regrouper les deux demandes ( dde infos périmètre + demande validation )
            // car plus simple pour l'utilisateur et le GRD et moins de manip !:
            // Mais le moment, on garde deux étapes séparées pour plus de clarté côté GRD.
            if (partage.EnergieType == PartageEnergieType.PairToPair &&
                partage.Perimetre is null)
            {
                return Result<DemandeValidationPartageDto>.Invalid(new ValidationError(
                    "Perimetre",
                    "Le périmètre doit être confirmé avant de demander la validation du partage.",
                    "Partage.PerimetreObligatoireAvantValidation",
                    ValidationSeverity.Error));
            }

            // 7. Évite de créer plusieurs ddes de validation en attente pour le même partage.
            var demandeValidationEnAttente = partage.DemandesGrd.Any(d =>
                d.DemandeType == DemandeGRDType.NouvelleActivation &&
                d.ResponseStatus == DdeGRDResponseStatus.EnAttente);

            if (demandeValidationEnAttente)
            {
                return Result<DemandeValidationPartageDto>.Invalid(new ValidationError(
                    "DemandeGRD",
                    "Une demande de validation est déjà en attente pour ce partage.",
                    "DemandeGRD.ValidationDejaEnAttente",
                    ValidationSeverity.Error));
            }

            // 8. Evolution du statut du partage vers EnAttenteValidation.
            // Cette méthode contient la règle métier côté domaine.
            var soumissionResult = partage.SoumettreNouveauPartageAuGrd();

            if (!soumissionResult.IsSuccess)
                return Result<DemandeValidationPartageDto>.Invalid(soumissionResult.ValidationErrors);

            // 9. Prépare le texte qui sera visible par le GRD.
            // On y place un résumé du partage + les points d'accès des participants.
            var detailsDemande = BuildDetailsDemandeValidation(partage);

            // 10. Crée la demande GRD via la factory --> Cette dernière garantit que la demande est créée avec ! :
            // - un type correct : NouvelleActivation
            // - un statut : EnAttente
            // - un demandeur
            // - un partage lié
            var demandeResult = DemandeGRD.CreateDemandeValidationNouveauPartage(
                partage.Id,
                userId,
                detailsDemande);

            if (!demandeResult.IsSuccess)
                return Result<DemandeValidationPartageDto>.Invalid(demandeResult.ValidationErrors);

            var demande = demandeResult.Value;

            // 11. Ajoute la demande au partage --> permet au domaine Partage de refuser l'ajout si son état ne le permet pas.
            var ajoutResult = partage.AjouterDemandeGrd(demande);

            if (!ajoutResult.IsSuccess)
                return Result<DemandeValidationPartageDto>.Invalid(ajoutResult.ValidationErrors);

            // 12. On indique à EF que la dde est une nouvelle entité.
            // Pas de SaveChangesAsync car le UnitOfWorkBehavior  s'occupe de la sauvegarde automatiquement après le handler.
            await context.DemandesGRD.AddAsync(demande, cancellationToken);

            // 13. Retourne un DTO pour l'API/UI.
            return Result.Success(new DemandeValidationPartageDto(
                demande.Id,
                partage.Id,
                demande.DateDemande,
                demande.ResponseStatus.ToString(),
                demande.DetailsDemande ?? string.Empty
            ));
        }

        // Construction du texte transmis au GRD.
        // Ici version simple via la concaténation de quelques lignes lisibles.
        // Eventuellement StringBuilder serait utile pour de très gros textes, mais  pour le moment pas nécessaire.
        private static string BuildDetailsDemandeValidation(Domain.Entities.Partages.Partage partage)
        {
            var details =
                "Demande de validation d'un nouveau partage.\n\n" +
                $"Nom du partage : {partage.Nom}\n" +
                $"Type de partage : {partage.EnergieType}\n" +
                $"Périmètre : {partage.Perimetre?.ToString() ?? "Non défini"}\n" +
                $"Nombre de participants : {partage.NombreParticipants}\n\n" +
                "Points d'accès participants :\n";

            // On ajoute chaque membre actif avec son rôle et son adresse.
            // Les membres sortis du partage ne sont pas envoyés au GRD.
            foreach (var membre in partage.Membres.Where(m => m.ExitAt is null))
            {
                details +=
                    $"- {membre.UserRolePartage} : " +
                    $"{membre.PointAccess.AdresseLine1}, " +
                    $"{membre.PointAccess.CodePostal}\n";
            }

            return details;
        }
    }
}