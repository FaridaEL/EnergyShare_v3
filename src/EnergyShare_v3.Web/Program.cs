using EnergyShare_v3.Application.Features.Partage;
using EnergyShare_v3.Application.Features.Users;
using EnergyShare_v3.Application.Interfaces;
using EnergyShare_v3.Domain.Entities.Users;
using EnergyShare_v3.Infrastructure;
using EnergyShare_v3.Infrastructure.Authentication;
using EnergyShare_v3.Infrastructure.Database;
using EnergyShare_v3.Infrastructure.Services;
using EnergyShare_v3.Web.Components;
using EnergyShare_v3.Web.Endpoints;
using EnergyShare_v3.Web.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Net;
using System.Text;

//using Microsoft.OpenApi;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents(options =>
    {
        options.DetailedErrors = true;
    });
builder.Services.AddCascadingAuthenticationState();

// Configuration JWT (pour l'API uniquement)
// Ce projet utilise DEUX mécanismes d'authentification :
// - ASP.NET Identity (cookies) → pour l'interface Blazor Server
// - JWT Bearer → pour sécuriser les endpoints API (/api/*)
//
// Important :
// On n'utilise PAS JWT comme schéma par défaut afin de ne pas casser
// l'authentification cookie utilisée par Blazor.
// JWT est utilisé uniquement pour les appels API (Postman, mobile, etc.)

//L’application utilise ASP.NET Identity pour gérer l’authentification côté interface Blazor Server(basée sur cookies).
//En parallèle, une authentification JWT est mise en place pour sécuriser les endpoints API, permettant une utilisation future par des clients externes (mobile, SPA, etc.).
//Cette séparation permet de combiner confort d’utilisation côté web et extensibilité côté API.

builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("Jwt"));

builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

var jwtSection = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSection["SecretKey"]
    ?? throw new InvalidOperationException("Jwt:SecretKey est manquante.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtSection["Issuer"],

        ValidateAudience = true,
        ValidAudience = jwtSection["Audience"],

        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(secretKey)),

        ClockSkew = TimeSpan.FromMinutes(1)
    };
});



builder.Services.AddAuthorization(options =>
{        //Il s'agit de sipmle policies, les policies plus complexes sont définies dans le handler
         //cf. tableau des autorisations définies dans le cdc
    // Admin uniquement
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Administrateur"));

    // Admin ou OrganismePublic
    options.AddPolicy("AdminOrOrganismePublic", policy =>
        policy.RequireRole("Administrateur", "OrganismePublic"));

    // Tous les utilisateurs connectés
    options.AddPolicy("AuthenticatedUser", policy =>
        policy.RequireAuthenticatedUser());

    // Utilisateur standard ou admin
    options.AddPolicy("StandardUserOnly", policy =>
        policy.RequireRole("Utilisateur", "Administrateur"));

    // Voir toutes les demandes GRD
    options.AddPolicy("CanViewAllValidationRequests", policy =>
        policy.RequireRole("Administrateur", "OrganismePublic"));

    // Voir tous les users
    options.AddPolicy("CanViewAllUsers", policy =>
        policy.RequireRole("Administrateur"));

    // Créer un partage
    //Attention il faudra également vérifier via conditions métier que le profil énergie existe, point access existe, rolepartage est vendeur ou mixte
    options.AddPolicy("CanCreatePartage", policy =>
        policy.RequireRole("Utilisateur", "Administrateur"));

    // Ajouter un point d’accès / profil énergie
    options.AddPolicy("CanManageEnergyProfile", policy =>
        policy.RequireRole("Utilisateur", "Administrateur"));
});

//Ajout l'infrastructure (EF Core, DbContext)
// Un seul appel qui cache toute la complexite grace a la methode d'extension
builder.Services.AddEnergyShare(builder.Configuration);


// Exception handler
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();

/*builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "EnergyShare API",
        Version = "v1"
    });
});  */

//Enregistrer les handlers de l'application 
/*à vérifier ça me semble peu et contradiction entre le projet et les exos */
builder.Services.AddScoped<GetUsersHandler>();
builder.Services.AddScoped<GetPartagesHandler>();
builder.Services.AddScoped<GetPartageByIdHandler>();
builder.Services.AddScoped<CreatePartageHandler>();


var app = builder.Build();

// Creer la base de donnees automatiquement en developpement
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    //await context.Database.EnsureCreatedAsync(); // Todo à remplacer par MigrateAsync() lorsque je fais mes migrations

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

    await context.Database.MigrateAsync();
    await ApplicationDbContextSeeder.SeedAsync(context, userManager, roleManager);
    // app.UseSwagger();
    // app.UseSwaggerUI();

}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    //app.UseDeveloperExceptionPage();
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
//else { 
//    app.UseExceptionHandler();
//}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

//Minimal API
app.MapUsers();
app.MapAuth();

app.MapPost("/logout", async (SignInManager<User> signInManager) =>
{
    try
    {
        await signInManager.SignOutAsync();
        return Results.Redirect("/login");
    }
    catch (Exception ex)
    {
        return Results.Problem(
            title: "Erreur logout",
            detail: ex.ToString(),
            statusCode: 500);
    }
});

app.Run();

public partial class Program { }
