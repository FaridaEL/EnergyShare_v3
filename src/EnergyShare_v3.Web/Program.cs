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
using EnergyShare_v3.Web.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
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
// On n'utilise PAS JWT comme schéma par défaut afin de ne pas casser l'authentification cookie utilisée par Blazor.
// JWT est utilisé uniquement pour les appels API (Postman, mobile, etc.)

//L’application utilise ASP.NET Identity pour gérer l’authentification côté interface Blazor Server(basée sur cookies).
//En parallèle, une authentification JWT est mise en place pour sécuriser les endpoints API, permettant une utilisation future par des clients externes (mobile, SPA, etc.).
//Cette séparation permet de combiner confort d’utilisation côté web et extensibilité côté API.

if (builder.Environment.IsEnvironment("Testing"))
{
    builder.Configuration["Jwt:Issuer"] = "EnergyShare.Tests";
    builder.Configuration["Jwt:Audience"] = "EnergyShare.Tests";
    // En environnement de test, on fournit une clé JWT fictive.
    // Cela évite de dépendre des user-secrets pendant les tests d’intégration.
    builder.Configuration["Jwt:SecretKey"] = "TEST_SECRET_KEY_123456789012345678901234567890";
}


builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("Jwt"));

builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

var jwtSection = builder.Configuration.GetSection("Jwt");

var secretKey = jwtSection["SecretKey"];    //    La clé JWT est stockée via user-secrets en développement


if (string.IsNullOrWhiteSpace(secretKey))    
    throw new InvalidOperationException("Jwt:SecretKey est manquante.");

if (secretKey.Length < 32)
    throw new InvalidOperationException("Jwt:SecretKey est trop courte. Minimum 32 caractères.");
/*
builder.Services.AddAuthentication(
    options =>
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

*/

builder.Services.AddAuthentication()
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
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

//user-context --> facilite la récupération des informations du user connecté (ex: son Id) dans les handlers de l'application sans devoir injecter HttpContextAccessor partout
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserContext, CurrentUserContext>();

//audit-securité
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
    options.AccessDeniedPath = "/access-denied";

    // Pour les routes API, on ne veut pas de redirection HTML vers /login ou /access-denied.
    // Une API doit retourner un vrai code HTTP : 401 ou 403.
    options.Events.OnRedirectToLogin = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        }

        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };

    options.Events.OnRedirectToAccessDenied = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        }

        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
});

//audit-secrité  : limite le nombre de tentatives de connexion pour prévenir les attaques par force brute sur les endpoints d'authentification
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("login", limiterOptions =>
    {
        limiterOptions.PermitLimit = 5;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueLimit = 0;
    });

    options.RejectionStatusCode = 429;
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

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "EnergyShare API",
        Version = "v1",
        Description = "Documentation des endpoints API du projet EnergyShare"
    });

    // Pour tester les endpoints protégés par JWT dans Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Entrez votre token JWT ici."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
/*builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "EnergyShare API",
        Version = "v1"
    });
});  */

//Enregistrer les handlers de l'application 
/*à vérifier ça me semble peu et contradiction entre le projet et les exos
 * TODO : comme on utilise MEdiator --> ces handlers ne devraient pas être enregistrés manuellement.*/
builder.Services.AddScoped<GetUsersHandler>();
//builder.Services.AddScoped<GetPartagesHandler>();
//builder.Services.AddScoped<GetPartageByIdHandler>();
//builder.Services.AddScoped<CreatePartageHandler>();


var app = builder.Build();

// Creer la base de donnees automatiquement en developpement
if (app.Environment.IsDevelopment() && !app.Environment.IsEnvironment("Testing"))
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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "EnergyShare API v1");
        options.RoutePrefix = "swagger";
    });
}
// Configure the HTTP request pipeline.
app.UseExceptionHandler(); // Active le GlobalExceptionHandler --> centralise les exceptions non gérées et retourne des réponses propres.
if (!app.Environment.IsDevelopment())
{
    //app.UseDeveloperExceptionPage();
   // app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
//else { 
//    app.UseExceptionHandler();
//}

app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    await next();
});

//app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseWhen(  //pour les pages Blazor utiliser not found et pour les routes api laisser les codes d’erreur standards (ex: 404 pour ressource introuvable) afin que les clients API puissent les gérer correctement.
    context => !context.Request.Path.StartsWithSegments("/api"),
    branch =>
    {
        branch.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
    });

if (!app.Environment.IsEnvironment("Testing"))
{ app.UseHttpsRedirection();   //en prod/dev on force le https, mais pas en test pour faciliter les tests d’intégration sans devoir gérer les certificats.
}
app.UseRateLimiter();
app.UseMiddleware<CorrelationIdMiddelware>();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();



app.MapStaticAssets();

//Minimal API

//minimal api d'abord car MapRazorComponents<App>() peut capter des routes qui ne sont pas trouvées comme de la navigation Blazor. 
app.MapUsers();
app.MapProfilEnergie();
app.MapPointAccess();
app.MapMatching();
app.MapAuth();
app.MapDebug();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();



app.MapPost("/logout", async (SignInManager<User> signInManager) =>
{
    try
    {
        await signInManager.SignOutAsync();
        return Results.Redirect("/");
        //return Results.Redirect("/login");
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
