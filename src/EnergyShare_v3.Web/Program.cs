using EnergyShare_v3.Application.Features.Partage;
using EnergyShare_v3.Application.Features.Users;
using EnergyShare_v3.Infrastructure;
using EnergyShare_v3.Infrastructure.Database;
using EnergyShare_v3.Web.Components;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

//Ajout l'infrastructure (EF Core, DbContext)
// Un seul appel qui cache toute la complexite grace a la methode d'extension
builder.Services.AddEnergyShare(builder.Configuration);

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
    await context.Database.EnsureCreatedAsync(); // Todo à remplacer par MigrateAsync() lorsque je fais mes migrations
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
