//using EnergyShare_v3.Application.Interfaces;
//using EnergyShare_v3.Domain.Enums;
//using FluentValidation;
//using Mediator;

//namespace EnergyShare_v3.Application.Features.Users
//{       /*Service applicatif pour la gestion des utilisateurs.
//         Inscruption via Identiy et UseManager
//        Cette création de user est donc utile pour l'admin s'il doit créer des profils spécifiques*/
//    public record CreateUser(string Email):ICommand<Result<Guid>> ;
//    // public record CreateUserCommand(string Email,string PasswordHash,UserRole Role, UserType UserType);

//    public class CreateUserValidator : AbstractValidator<CreateUser>
//    {
//        public CreateUserValidator()
//        {
//            RuleFor(x => x.Email)
//                .NotEmpty()
//                .WithMessage("L'email est requis")
//                .EmailAddress()
//                .WithMessage("L'email n'est pas valide");
//        }
//    }


//    public class CreateUserHandler (IApplicationDbContext context) 
//        : ICommandHandler<CreateUser, Result<Guid>>
//    {
//        public async ValueTask<Result<Guid>> Handle(
//            CreateUser command,
//            CancellationToken cancellationToken )
//        {    var result = Domain.Entities.Users.User.Create( command.Email );

//            //Si erreur métier on s'arrête et on retourne l'erreur, sinon on continue
//            if (!result.IsSuccess)
//                return Result<Guid>.Invalid(result.ValidationErrors);

//            var user = result.Value;

//            //persistance
//            /*Attention : Remarquez que le handler n'appelle pas SaveChangesAsync(). 
//             * C'est le UnitOfWorkBehavior du pipeline qui s'en charge automatiquement. Cela garantit que le save se fait dans la transaction.*/
//            await context.Users.AddAsync(user, cancellationToken);
//           // await _context.SaveChangesAsync(cancellationToken);


//            return Result.Success(user.Id);

//        }

//    }
//}
    