using Ardalis.Result;
using EnergyShare_v3.Application.Interfaces;
using EnergyShare_v3.Domain.Enums;
using FluentValidation;
using Mediator;

namespace EnergyShare_v3.Application.Features.Users
{       /*Service applicatif pour la gestion des utilisateurs.
/ Requete pour obtenir la liste de toutes les familles..*/
    public record CreateUser(string Email,string PasswordHash,UserRole Role):ICommand<Result<Guid>> ;
    // public record CreateUserCommand(string Email,string PasswordHash,UserRole Role, UserType UserType);

    public class CreateUserValidator : AbstractValidator<CreateUser>
    {
        public CreateUserValidator()
        {
            RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("L'email est requis");

            RuleFor(x => x.PasswordHash)
                .NotEmpty()
                .WithMessage("Le mot de passe hashé est requis");

            RuleFor(x => x.Role)
                .IsInEnum();


        }
    }


    public class CreateUserHandler (IApplicationDbContext context) : ICommandHandler<CreateUser, Result<Guid>>
    {
        public async ValueTask<Result<Guid>> Handle(
            CreateUser command,
            CancellationToken cancellationToken )
        {    var result = Domain.Entities.Users.User.Create(
                command.Email,
                command.Role );

            //Si erreur métier on s'arrête et on retourne l'erreur, sinon on continue
            if (!result.IsSuccess)
                return Result<Guid>.Invalid(result.ValidationErrors);

            var user = result.Value;

            //persistance
            await context.Users.AddAsync(user, cancellationToken);
           // await _context.SaveChangesAsync(cancellationToken);

            return Result.Success(user.Id);

        }

    }
}
