using EnergyShare_v3.Application.Features.Partage;
using EnergyShare_v3.Domain.Enums;
using EnergyShare_v3.IntegrationTests.Common;
using EnergyShare_v3.Web.Models.Partage;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace EnergyShare_v3.IntegrationTests.Partage;

public class PartageUpdateEndpointTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly TestDataFactory _dataFactory;

    public PartageUpdateEndpointTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _dataFactory = new TestDataFactory(_client);
    }

    /// <summary>
    /// Vérifie qu'un utilisateur non authentifié ne peut pas modifier un partage.
    /// </summary>
    [Fact]
    public async Task UpdatePartage_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        var request = new UpdatePartageRequest(
            Nom: "Update interdit",
            Description: "Non authentifié",
            EnergieType: PartageEnergieType.PairToPair,
            DateDebut: new DateTime(2026, 6, 1),
            DateFin: null);

        var response = await _client.PutAsJsonAsync(
            $"/api/partages/{Guid.NewGuid()}",
            request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>
    /// Vérifie que le vendeur/interlocuteur unique peut modifier son propre partage.
    ///
    /// On utilise un vendeur dynamique créé par TestDataFactory afin d'éviter
    /// les conflits avec les utilisateurs seedés comme Sarah.
    /// </summary>
    [Fact]
    public async Task UpdatePartage_WhenUserIsSeller_ShouldReturnSuccess()
    {
        await _dataFactory.CreateSellerWithInjectionPointAsync();

        var partageId = await CreatePartageAsync("Partage à modifier");

        var request = new UpdatePartageRequest(
            Nom: "Partage modifié integration",
            Description: "Description modifiée depuis test intégration",
            EnergieType: PartageEnergieType.MemeBatiment,
            DateDebut: new DateTime(2026, 6, 1),
            DateFin: new DateTime(2026, 12, 31));

        var response = await _client.PutAsJsonAsync(
            $"/api/partages/{partageId}",
            request);

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync(
            $"/api/partages/{partageId}");

        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var dto = await getResponse.Content
            .ReadFromJsonAsync<PartageDetailsDto>();

        dto.Should().NotBeNull();
        dto!.Nom.Should().Be("Partage modifié integration");
        dto.Description.Should().Be("Description modifiée depuis test intégration");
        dto.EnergieType.Should().Be(PartageEnergieType.MemeBatiment);
        dto.DateDebut.Should().Be(new DateTime(2026, 6, 1));
        dto.DateFin.Should().Be(new DateTime(2026, 12, 31));
        dto.UpdatedAt.Should().NotBe(default(DateTime));
    }

    /// <summary>
    /// Vérifie qu'un utilisateur connecté mais non membre/vendeur du partage
    /// ne peut pas modifier le partage d'un autre utilisateur.
    /// </summary>
    [Fact]
    public async Task UpdatePartage_WhenUserIsNotSeller_ShouldReturnForbidden()
    {
        await _dataFactory.CreateSellerWithInjectionPointAsync();

        var partageId = await CreatePartageAsync("Partage sécurisé");

        await TestAuthHelper.AuthenticateAsync(
            _client,
            TestUsers.Julien);

        var request = new UpdatePartageRequest(
            Nom: "Modification interdite",
            Description: "Julien ne peut pas modifier",
            EnergieType: PartageEnergieType.MemeBatiment,
            DateDebut: new DateTime(2026, 6, 1),
            DateFin: null);

        var response = await _client.PutAsJsonAsync(
            $"/api/partages/{partageId}",
            request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    /// <summary>
    /// Vérifie la validation métier sur les dates :
    /// la date de fin ne peut pas être antérieure à la date de début.
    /// </summary>
    [Fact]
    public async Task UpdatePartage_WhenDateFinIsBeforeDateDebut_ShouldReturnBadRequest()
    {
        await _dataFactory.CreateSellerWithInjectionPointAsync();

        var partageId = await CreatePartageAsync("Partage validation dates");

        var request = new UpdatePartageRequest(
            Nom: "Partage validation dates",
            Description: "Dates invalides",
            EnergieType: PartageEnergieType.PairToPair,
            DateDebut: new DateTime(2026, 12, 31),
            DateFin: new DateTime(2026, 6, 1));

        var response = await _client.PutAsJsonAsync(
            $"/api/partages/{partageId}",
            request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Helper local : crée un partage avec l'utilisateur actuellement authentifié.
    /// Le HttpClient doit donc déjà être authentifié avant d'appeler cette méthode.
    /// </summary>
    private async Task<Guid> CreatePartageAsync(string nom)
    {
        var createCommand = new CreatePartage(
            Nom: nom,
            EnergieType: PartageEnergieType.PairToPair);

        var createResponse = await _client.PostAsJsonAsync(
            "/api/partages",
            createCommand);

        var body = await createResponse.Content.ReadAsStringAsync();

        createResponse.StatusCode.Should().Be(
            HttpStatusCode.Created,
            $"Création du partage échouée. Réponse API : {body}");

        var partageId = await createResponse.Content
            .ReadFromJsonAsync<Guid>();

        partageId.Should().NotBeEmpty();

        return partageId;
    }
}