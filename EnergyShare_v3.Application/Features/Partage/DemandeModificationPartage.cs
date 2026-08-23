using Ardalis.Result;
using EnergyShare_v3.Application.Interfaces;
using EnergyShare_v3.Domain.Entities.Partages;
using EnergyShare_v3.Domain.Enums;
using Mediator;
using Microsoft.EntityFrameworkCore;
//____________________________________________
//MODIFICATION DU PARTAGE PAR LE GRD  POST VALIDATION INITIALE
//____________________________________________
namespace EnergyShare_v3.Application.Features.Partage
{
    // Commande envoyée quand le vendeur clique sur "Demander validation GRD".
    // Elle contient uniquement l'Id du partage, car l'utilisateur connecté est récupéré via IUserContext.
    public record DemandeModificationPartage(Guid PartageId)
        : ICommand<Result<DemandeModificationPartageDto>>;

    public class DemandeModificationPartageHandler(
        IApplicationDbContext context,
        IUserContext userContext)
        : ICommandHandler<DemandeModificationPartage, Result<DemandeModificationPartageDto>>
    {
        public async ValueTask<Result<DemandeModificationPartageDto>> Handle(
            DemandeModificationPartage command,
            CancellationToken cancellationToken)
        {
            // 1. Vérifie qu’un utilisateur est connecté.
            if (!userContext.IsAuthenticated || userContext.UserId is null)
                return Result<DemandeModificationPartageDto>.Unauthorized();

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
                return Result<DemandeModificationPartageDto>.NotFound();

            // 3. Seul le vendeur / créateur du partage peut déclarer une modification. 
            if (partage.VendeurId != userId)
                return Result<DemandeModificationPartageDto>.Forbidden();

            // 4. Une demande de modification concerne uniquement un partage déjà actif ou suspendu :
            //if (partage.Statut != PartageEnergieStatutType.Actif)
            //{
            //    return Result<DemandeModificationPartageDto>.Conflict(
            //        "Seul un partage actif peut faire l'objet d'une demande de modification.");
            //}


            // 4. Une demande de modification peut être introduite :
            // - depuis un partage actif ;
            // - ou depuis un partage suspendu après correction des éléments refusés par le GRD.
            if (partage.Statut != PartageEnergieStatutType.Actif && partage.Statut != PartageEnergieStatutType.Suspendu)
            {
                return Result<DemandeModificationPartageDto>.Conflict(
                    "Le partage doit être actif ou suspendu pour faire l'objet d'une demande de modification.");
            }
            var validationMembres = partage.VerifierNombreMembres();

            if (!validationMembres.IsSuccess)
                return Result<DemandeModificationPartageDto>.Invalid(validationMembres.ValidationErrors);


            // 5. Evite plusieurs demandes de modification simultanées.
            var demandeModificationEnAttente = partage.DemandesGrd.Any(d =>
                  d.DemandeType == DemandeGRDType.ModificationPartageExistant &&
                  d.ResponseStatus == DdeGRDResponseStatus.EnAttente);


            if (demandeModificationEnAttente)
            {
                return Result<DemandeModificationPartageDto>.Invalid(new ValidationError(
                    "DemandeGRD",
                    "Une demande de modification est déjà en attente pour ce partage.",
                    "DemandeGRD.ModificationDejaEnAttente",
                    ValidationSeverity.Error));
            }

            // 6 Evolution du statut du partage vers EnAttenteModification.
            // Cette méthode contient la règle métier côté domaine.
            var modificationResult = partage.DemanderModification();

            if (!modificationResult.IsSuccess)
                return Result<DemandeModificationPartageDto>.Invalid(modificationResult.ValidationErrors);

            // 7. Prépare le texte qui sera visible par le GRD.
            // On y place un résumé du partage + les points d'accès des participants.
            var detailsDemande = BuildDetailsDemandeModification(partage);

            // 8. Crée la demande GRD via la factory --> Cette dernière garantit que la demande est créée avec ! :
           
            var demandeResult = DemandeGRD.CreateDemandeModificationPartage(
                partage.Id,
                userId,
                detailsDemande);

            if (!demandeResult.IsSuccess)
                return Result<DemandeModificationPartageDto>.Invalid(demandeResult.ValidationErrors);

            var demande = demandeResult.Value;

            // 9. Ajoute la demande au partage --> permet au domaine Partage de refuser l'ajout si son état ne le permet pas.
            var ajoutResult = partage.AjouterDemandeGrd(demande);

            if (!ajoutResult.IsSuccess)
                return Result<DemandeModificationPartageDto>.Invalid(ajoutResult.ValidationErrors);

            // 10. On indique à EF que la dde est une nouvelle entité.
            // Pas de SaveChangesAsync car le UnitOfWorkBehavior  s'occupe de la sauvegarde automatiquement après le handler.
            await context.DemandesGRD.AddAsync(demande, cancellationToken);

            // 11. Retourne un DTO pour l'API/UI.
            return Result.Success(new DemandeModificationPartageDto(
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
        private static string BuildDetailsDemandeModification(Domain.Entities.Partages.Partage partage)
        {
            var details =
                "Demande de modification d'un partage existant.\n\n" +
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