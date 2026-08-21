using Ardalis.Result;
using EnergyShare_v3.Application.Interfaces;
using EnergyShare_v3.Domain.Entities.Partages;
using EnergyShare_v3.Domain.Enums;
using FluentValidation;
using Mediator;
using Microsoft.EntityFrameworkCore;
using System.Drawing;

namespace EnergyShare_v3.Application.Features.Partage
{
    public record CreatePartage(
     string Nom,
     PartageEnergieType EnergieType,
     Guid PointAccessId
    // Guid VendeurId
 ) : ICommand<Result<Guid>>;

    public class CreatePartageValidator : AbstractValidator<CreatePartage>
    {
        public CreatePartageValidator()
        {
            RuleFor(x => x.Nom)
                .NotEmpty()
                .WithMessage("Le nom du partage est requis")
                .MaximumLength(100);

            RuleFor(x => x.PointAccessId)
                .NotEmpty()
                .WithMessage("Le point d'accès est requis");
            /*
            RuleFor(x => x.VendeurId)
                .NotEmpty()
                .WithMessage("Le vendeur est requis");  */
        }
    }

    public class CreatePartageHandler(
        IApplicationDbContext context,
        IUserContext userContext)   // on récupère l'Id du user connecté via IUserContext pour l'associer au partage créé
        : ICommandHandler<CreatePartage, Result<Guid>>
    {
        public async ValueTask<Result<Guid>> Handle(
            CreatePartage command,
            CancellationToken cancellationToken)
        {
            // Vérifie qu’un utilisateur est bien connecté
            if (!userContext.IsAuthenticated || userContext.UserId is null)
                return Result<Guid>.Unauthorized();

            var vendeurId = userContext.UserId.Value; // Utilise l'Id de l'utilisateur connecté comme vendeur du partage  via user-context

            // fix : ajouter le vendeur dans participation partage
            // condition prélable : Un vendeur doit avoir un point d’accès actif qui injecte de l’énergie.
            // C’est ce point d’accès qui sera ajouté comme première participation du partage.
            var pointInjectionVendeur = await context.PointAccesses
                .FirstOrDefaultAsync(p =>
                    p.Id == command.PointAccessId &&  // on récupère le point d’accès sélectionné par le vendeur dans la commande et non plus le premier de la liste !
                    p.UserId == vendeurId &&
                    p.EstActif &&
                    p.IsInjectionPoint,
                    cancellationToken);

            if (pointInjectionVendeur is null)
            {
                return Result<Guid>.Invalid(new ValidationError(
                    "PointAccess",
                    "Le point d'accès sélectionné n'est pas valide ou n'est pas disponible pour créer un partage.",
                    //"Vous devez d’abord déclarer un point d’accès d’injection actif pour créer un partage.",
                    "CreatePartage.PointInjectionObligatoire",
                    ValidationSeverity.Error));
            }

            //Fix : 1 EAN = 1 partage ( Sf si clôturé) 
            var pointDejaDansUnPartage = await context.MembresPartage
            .AnyAsync(mp =>
                mp.PointAccessId == pointInjectionVendeur.Id
                && mp.Partage.Statut != PartageEnergieStatutType.Cloture,
                cancellationToken);

                    if (pointDejaDansUnPartage)
                    {
                        return Result<Guid>.Invalid(new ValidationError(
                            "PointAccess",
                            "Ce point d’accès appartient déjà à un partage d’énergie. Vous ne pouvez pas créer un nouveau partage avec le même point d’accès.",
                            "CreatePartage.PointAccessDejaUtilise",
                            ValidationSeverity.Error));
                    }



            var result = Domain.Entities.Partages.Partage.Create(
                command.Nom,
                command.EnergieType,
                vendeurId);

            if (!result.IsSuccess)
                return Result<Guid>.Invalid(result.ValidationErrors);

            var partage = result.Value;

            // fix : ajouter le vendeur dans participation partage + interlocuteur unique
            // Ainsi, NombreParticipants inclut bien le créateur du partage.
            var participationVendeurResult = ParticipationPartage.Create(
                partage.Id,
                pointInjectionVendeur.Id,
                UserRolePartage.Vendeur);

            if (!participationVendeurResult.IsSuccess)
                return Result<Guid>.Invalid(participationVendeurResult.ValidationErrors);

            var participationVendeur = participationVendeurResult.Value;

            // Le vendeur est l’interlocuteur unique vis-à-vis du GRD.
            //participationVendeur.DefinirCommeInterlocuteurUnique();

            var interlocuteurResult = participationVendeur.DefinirCommeInterlocuteurUnique();

            if (!interlocuteurResult.IsSuccess)
                return Result<Guid>.Invalid(interlocuteurResult.ValidationErrors);

            var ajoutResult = partage.AjouterMembre(participationVendeur);

            if (!ajoutResult.IsSuccess)
                return Result<Guid>.Invalid(ajoutResult.ValidationErrors);

            await context.Partages.AddAsync(partage, cancellationToken);

            // Important : on indique explicitement à EF que la participation est nouvelle.
            // Le SaveChanges est géré par le UnitOfWorkBehavior.
            await context.MembresPartage.AddAsync(participationVendeur, cancellationToken);

            return Result.Success(partage.Id);
        }
    }
}
