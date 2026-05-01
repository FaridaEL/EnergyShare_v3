using Ardalis.Result;
using EnergyShare_v3.Application.Interfaces;
using EnergyShare_v3.Domain.Entities.Users;
using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EnergyShare_v3.Application.Features.Users
{
    public record UpdateMyUserProfile(
        string? FirstName,
        string? LastName,
        string? PhoneNumber,
        string? SocieteName,
        string? NumeroEntreprise
    ) : ICommand<Result>;

    public class UpdateMyUserProfileValidator : AbstractValidator<UpdateMyUserProfile>
    {
        public UpdateMyUserProfileValidator()
        {
            RuleFor(x => x.FirstName)
                .MaximumLength(100);

            RuleFor(x => x.LastName)
                .MaximumLength(100);

            RuleFor(x => x.PhoneNumber)
                .MaximumLength(50);

            RuleFor(x => x.SocieteName)
                .MaximumLength(200);

            RuleFor(x => x.NumeroEntreprise)
                .Matches(@"^BE\d{10}$")
                .When(x => !string.IsNullOrWhiteSpace(x.NumeroEntreprise))
                .WithMessage("Format invalide : le numéro d'entreprise doit commencer par BE suivi de 10 chiffres.");
        }
    }

    public class UpdateMyUserProfileHandler(
        IApplicationDbContext context,
        IUserContext userContext)
        //UserManager<User> userManager)
        : ICommandHandler<UpdateMyUserProfile, Result>
    {
        public async ValueTask<Result> Handle(
            UpdateMyUserProfile command,
            CancellationToken cancellationToken)
        {
            //debug
            Console.WriteLine($"DEBUG HANDLER - FirstName = {command.FirstName}");
            Console.WriteLine($"DEBUG HANDLER - LastName = {command.LastName}");
            Console.WriteLine($"DEBUG HANDLER - PhoneNumber = {command.PhoneNumber}");
            Console.WriteLine($"DEBUG HANDLER - SocieteName = {command.SocieteName}");
            Console.WriteLine($"DEBUG HANDLER - NumeroEntreprise = {command.NumeroEntreprise}");

            var userId = userContext.UserId;

            if (userId == Guid.Empty)
                return Result.Unauthorized();

            var user = await context.Users
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

            if (user is null)
                return Result.NotFound("Utilisateur introuvable.");

            user.UpdateUserIdentity(
                command.FirstName,
                command.LastName,
                command.PhoneNumber);

            var legalResult = user.UpdateLegalInformation(
                command.SocieteName,
                command.NumeroEntreprise);

            //DEbug
            Console.WriteLine($"DEBUG USER BEFORE SAVE - SocieteName = {user.SocieteName}");
            Console.WriteLine($"DEBUG USER BEFORE SAVE - NumeroEntreprise = {user.NumeroEntreprise}");

            if (!legalResult.IsSuccess)
                return legalResult;

            // !  gestion de la persistance avec Identity
            // ------------------------------------------------------
            // Dans l'app, la persistance est gérée via IApplicationDbContext et UnitOfWorkBehavior (SaveChangesAsync automatique en fin de pipeline).
            //
            // Cependant, pour l'entité User qui hérite de IdentityUser, on utilise UserManager (Identity) pour les opérations de mise à jour.
            //
            // Pourquoi ?
            // - UserManager gère correctement les champs liés à l'authentification (sécurité, normalisation, etc.)
            // - Il utilise son propre mécanisme de persistance via EF Core (UserStore)
            // - Il garantit la cohérence avec le système Identity
            //
            // Donc en ccl :
            // → Pour les entités métier : context + UnitOfWork
            // → Pour les utilisateurs : UserManager (ne pas doubler avec context.SaveChanges)

            //Finalement :
            // Ici, on ne fait pas userManager.UpdateAsync(user).
            // Le user a été chargé via IApplicationDbContext, donc EF Core le suit déjà.
            // La sauvegarde sera effectuée par le UnitOfWorkBehavior en fin de pipeline.
            // Cela évite de mélanger deux mécanismes de persistance pour une simple mise à jour de profil.

            //var identityResult = await userManager.UpdateAsync(user);

            //if (!identityResult.Succeeded)
            //{
            //    return Result.Invalid(
            //        identityResult.Errors.Select(e => new ValidationError
            //        {
            //            Identifier = e.Code,
            //            ErrorMessage = e.Description
            //        }));
            //}

            return Result.Success();
        }
    }
}