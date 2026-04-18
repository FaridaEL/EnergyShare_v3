using EnergyShare_v3.Application.Interfaces;
using EnergyShare_v3.Domain.Entities.Users;
using EnergyShare_v3.Domain.Enums;
using EnergyShare_v3.Infrastructure.Authentication;
using EnergyShare_v3.Web.Models.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace EnergyShare_v3.Web.Endpoints
{          /*Jwt*/
    public static class AuthEndpoint
    {
        public static IEndpointRouteBuilder MapAuth(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/auth")
                .WithTags("Auth");

            group.MapPost("/register", Register).AllowAnonymous();   // crée le user+role identity+ token et refresh token 
            group.MapPost("/login", Login).AllowAnonymous();   //vérifie email+pwd
            group.MapPost("/refresh", Refresh).AllowAnonymous();  //vérifie le refresh token, révoque l'ancien, en génère un nouveau + access token
            group.MapPost("/logout", Logout).RequireAuthorization();  //récupère le user connecté viaJwt, révoque tout ses refreshtoken

            return app;
        }

        private static async Task<IResult> Register(
            [FromBody] RegisterRequest request,
            UserManager<User> userManager,
            IJwtTokenService tokenService,
            IApplicationDbContext dbContext,
            IOptions<JwtSettings> jwtOptions)
        {
            var userResult = User.Create(request.Email, UserRole.Utilisateur);

            if (!userResult.IsSuccess)
                return Results.BadRequest(userResult.Errors);

            var user = userResult.Value;
            user.UpdateUserIdentity(request.FirstName, request.LastName, null);

            var createResult = await userManager.CreateAsync(user, request.Password);

            if (!createResult.Succeeded)
                return Results.BadRequest(createResult.Errors);

            // Identity = source de vérité pour l'autorisation
            var roleResult = await userManager.AddToRoleAsync(user, "Utilisateur");
            if (!roleResult.Succeeded)
                return Results.BadRequest(roleResult.Errors);

            var roles = await userManager.GetRolesAsync(user);

            var accessToken = tokenService.GenerateAccessToken(user, roles);
            var refreshTokenValue = tokenService.GenerateRefreshToken();

            var refreshToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = refreshTokenValue,
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(jwtOptions.Value.RefreshTokenExpirationDays),
                IsRevoked = false
            };

            dbContext.RefreshTokens.Add(refreshToken);
            await dbContext.SaveChangesAsync();

            return Results.Ok(new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshTokenValue,
                AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(jwtOptions.Value.AccessTokenExpirationMinutes)
            });
        }

        private static async Task<IResult> Login(
            [FromBody] LoginRequest request,
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IJwtTokenService tokenService,
            IApplicationDbContext dbContext,
            IOptions<JwtSettings> jwtOptions)
        {
            var user = await userManager.FindByEmailAsync(request.Email);

            if (user is null)
                return Results.Unauthorized();

            var passwordResult = await signInManager.CheckPasswordSignInAsync(
                user,
                request.Password,
                lockoutOnFailure: true);

            if (!passwordResult.Succeeded)
                return Results.Unauthorized();

            var roles = await userManager.GetRolesAsync(user);

            var accessToken = tokenService.GenerateAccessToken(user, roles);
            var refreshTokenValue = tokenService.GenerateRefreshToken();

            var refreshToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = refreshTokenValue,
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(jwtOptions.Value.RefreshTokenExpirationDays),
                IsRevoked = false
            };

            dbContext.RefreshTokens.Add(refreshToken);
            await dbContext.SaveChangesAsync();

            return Results.Ok(new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshTokenValue,
                AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(jwtOptions.Value.AccessTokenExpirationMinutes)
            });
        }

        private static async Task<IResult> Refresh(
            [FromBody] RefreshRequest request,
            UserManager<User> userManager,
            IJwtTokenService tokenService,
            IApplicationDbContext dbContext,
            IOptions<JwtSettings> jwtOptions)
        {
            var existingRefreshToken = await dbContext.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken);

            if (existingRefreshToken is null)
                return Results.Unauthorized();

            if (existingRefreshToken.IsRevoked)
                return Results.Unauthorized();

            if (existingRefreshToken.ExpiresAt <= DateTime.UtcNow)
                return Results.Unauthorized();

            // Rotation : on révoque l'ancien
            existingRefreshToken.IsRevoked = true;

            var user = existingRefreshToken.User;
            var roles = await userManager.GetRolesAsync(user);

            var newAccessToken = tokenService.GenerateAccessToken(user, roles);
            var newRefreshTokenValue = tokenService.GenerateRefreshToken();

            var newRefreshToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = newRefreshTokenValue,
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(jwtOptions.Value.RefreshTokenExpirationDays),
                IsRevoked = false
            };

            dbContext.RefreshTokens.Add(newRefreshToken);
            await dbContext.SaveChangesAsync();

            return Results.Ok(new AuthResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshTokenValue,
                AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(jwtOptions.Value.AccessTokenExpirationMinutes)
            });
        }

        private static async Task<IResult> Logout(
            ClaimsPrincipal principal,
            IApplicationDbContext dbContext)
        {
            var userIdClaim = principal.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!Guid.TryParse(userIdClaim, out var userId))
                return Results.Unauthorized();

            var activeRefreshTokens = await dbContext.RefreshTokens
                .Where(rt => rt.UserId == userId && !rt.IsRevoked)
                .ToListAsync();

            foreach (var token in activeRefreshTokens)
            {
                token.IsRevoked = true;
            }

            await dbContext.SaveChangesAsync();

            return Results.Ok();
        }
    }
}
