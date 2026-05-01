using Ardalis.Result;
using EnergyShare_v3.Application.Interfaces;
using EnergyShare_v3.Domain.Entities.Users;
using Mediator;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EnergyShare_v3.Application.Features.Users
{
    public record GetMyUserProfile()
       : IQuery<Result<CurrentUserProfileDto>>;

    public class GetMyUserProfileHandler(
        IApplicationDbContext context,
        IUserContext userContext,
        UserManager<User> userManager)
        : IQueryHandler<GetMyUserProfile, Result<CurrentUserProfileDto>>
    {
        public async ValueTask<Result<CurrentUserProfileDto>> Handle(
            GetMyUserProfile query,
            CancellationToken cancellationToken)
        {
            var userId = userContext.UserId;

            if (userId == Guid.Empty)
                return Result<CurrentUserProfileDto>.Unauthorized();

            var user = await context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

            if (user is null)
                return Result<CurrentUserProfileDto>.NotFound("Utilisateur introuvable.");

            var roles = await userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "Utilisateur";

            var dto = new CurrentUserProfileDto(
                user.Id,
                user.Email ?? string.Empty,
                user.FirstName,
                user.LastName,
                user.PhoneNumber,
                user.SocieteName,
                user.NumeroEntreprise,
                user.Status.ToString(),
                role
            );

            return Result.Success(dto);
        }
    }
}
