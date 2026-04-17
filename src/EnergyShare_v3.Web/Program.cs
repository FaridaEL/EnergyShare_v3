using EnergyShare_v3.Application.Features.Partage;
using EnergyShare_v3.Application.Features.Users;
using EnergyShare_v3.Domain.Entities.Users;
using EnergyShare_v3.Infrastructure;
using EnergyShare_v3.Infrastructure.Database;
using EnergyShare_v3.Web.Components;
using EnergyShare_v3.Web.Endpoints;
using EnergyShare_v3.Web.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
//using Microsoft.OpenApi;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents(options =>
    {
        options.DetailedErrors = true;
    });
builder.Services.AddCascadingAuthenticationState();

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
