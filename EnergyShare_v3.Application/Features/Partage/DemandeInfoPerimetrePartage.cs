using Ardalis.Result;
using EnergyShare_v3.Application.Interfaces;
using EnergyShare_v3.Domain.Entities.Partages;
using Mediator;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace EnergyShare_v3.Application.Features.Partage
{
    // But : la commande est appelée lorsque le créateur du partage clique sur "Demander info périmètre".
    // Le DTO retournée permet d'afficher dans l'UI un message clair ou de tracer la demande créée.
    public record DemandeInfoPerimetrePartage(Guid PartageId)
        : ICommand<Result<DemandePerimetreDto>>;

    public class DemandeInfoPerimetrePartageHandler(
        IApplicationDbContext context,
        IUserContext userContext)
        : ICommandHandler<DemandeInfoPerimetrePartage, Result<DemandePerimetreDto>>
    {
        public async ValueTask<Result<DemandePerimetreDto>> Handle(
            DemandeInfoPerimetrePartage command,
            CancellationToken cancellationToken)
        {
            // Vérifie qu’un utilisateur est connecté.
            if (!userContext.IsAuthenticated || userContext.UserId is null)
                return Result<DemandePerimetreDto>.Unauthorized();

            var userId = userContext.UserId.Value;

            // On charge le partage ses membres et leurs points d’accès!
            // Les membres sont nécessaires pour vérifier que le partage est complet.
            // Les PointAccess sont utiles pou récupérer les adresses --> on évite une étape de copier-coller
            // manuel pour le vendeur
            var partage = await context.Partages
                .Include(p => p.Membres)
                    .ThenInclude(m => m.PointAccess)
                .Include(p => p.DemandesGrd)
                .FirstOrDefaultAsync(p => p.Id == command.PartageId, cancellationToken);

            if (partage is null)
                return Result<DemandePerimetreDto>.NotFound();

            // Seul le vendeur / créateur du partage peut demander les informations de périmètre.
            if (partage.VendeurId != userId)
                return Result<DemandePerimetreDto>.Forbidden();

            // Validation du nombre de membres selon le type de partage.
            // PairToPair : exactement 2 membres.
            // Même bâtiment : au moins 2 membres.
            // on revalide meme si déjà fait lors de la création, car : entre temps un membre peut avoir par ex quitté le partage, etc..
            var validationMembres = partage.VerifierNombreMembres();

            if (!validationMembres.IsSuccess)
                return Result<DemandePerimetreDto>.Invalid(validationMembres.ValidationErrors);

            // On génère automatiquement le contenu de la demande via notre modèle : 
            // Partage -> ParticipationPartage -> PointAccess -> Adresse.
            var adressesMembres = partage.Membres
                .Where(m => m.ExitAt == null)
                .Select(m => $"- {m.PointAccess.AdresseLine1}, {m.PointAccess.CodePostal}")
                .ToList();

            var detailsDemande =
                "Demande d'information de périmètre pour le partage.\n\n" +
                "Adresses des points d’accès concernés :\n" +
                string.Join("\n", adressesMembres);

            // Création de la demande via la factory de DemandeGRD qu garantit que la dde est créée dans
            // un état cohérent : --> statut EnAttente, type DdeInfoPerimetre, date de demande, demandeur, partage.
            var demandeResult = DemandeGRD.CreateDemandeInfoPerimetre(
                partage.Id,
                userId,
                detailsDemande);

            if (!demandeResult.IsSuccess)
                return Result<DemandePerimetreDto>.Invalid(demandeResult.ValidationErrors);

            var demande = demandeResult.Value;

            // Ajout via Partage --> permet à Partage de refuser l’ajout si son état métier ne le permet pas
            // ex : partage clôturé ou en cours de clôture.
            var ajoutResult = partage.AjouterDemandeGrd(demande);

            if (!ajoutResult.IsSuccess)
                return Result<DemandePerimetreDto>.Invalid(ajoutResult.ValidationErrors);
            // Important : on indique explicitement à EF que la demande GRD est une nouvelle entité.
            // Ce n’est pas un SaveChanges : la persistance finale reste gérée par le UnitOfWorkBehavior
            await context.DemandesGRD.AddAsync(demande, cancellationToken);

            if (!ajoutResult.IsSuccess)
                return Result<DemandePerimetreDto>.Invalid(ajoutResult.ValidationErrors);

            // Pas de SaveChangesAsync ici : le UnitOfWorkBehavior sauvegarde automatiquement après le handler.

            return Result.Success(new DemandePerimetreDto(
                demande.Id,
                partage.Id,
                demande.DateDemande,
                demande.ResponseStatus.ToString(),
                demande.DetailsDemande ?? string.Empty
            ));
        }
    }
}