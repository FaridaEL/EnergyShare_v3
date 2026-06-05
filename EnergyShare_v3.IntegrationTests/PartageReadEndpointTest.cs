using EnergyShare_v3.Application.Features.Partage;
using EnergyShare_v3.Domain.Enums;
using EnergyShare_v3.IntegrationTests.Common;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace EnergyShare_v3.IntegrationTests.Partage;

public class PartageReadEndpointTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly TestDataFactory _dataFactory;

    public PartageReadEndpointTests(
        CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _dataFactory = new TestDataFactory(_client);
    }

    /// <summary>
    /// Vérifie qu'un vendeur peut consulter le détail d'un partage qu'il vient de créer.
    ///
    /// Utilisation d'un utilisateur dynamique afin d'éviter les conflits avec les données seedées.
    /// </summary>
    [Fact]
    public async Task GetPartageById_WhenUserIsSeller_ShouldReturnOk()
    {
        await _dataFactory.CreateSellerWithInjectionPointAsync();

        var command = new CreatePartage(
            Nom: "Partage Seller Access Test",
            EnergieType: PartageEnergieType.PairToPair);

        var createResponse =
            await _client.PostAsJsonAsync(
                "/api/partages",
                command);

        var createBody =
            await createResponse.Content.ReadAsStringAsync();

        createResponse.StatusCode.Should().Be(
            HttpStatusCode.Created,
            $"Réponse API : {createBody}");

        var partageId =
            await createResponse.Content.ReadFromJsonAsync<Guid>();

        var response =
            await _client.GetAsync(
                $"/api/partages/{partageId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var dto =
            await response.Content
                .ReadFromJsonAsync<PartageDetailsDto>();

        dto.Should().NotBeNull();

        dto!.Id.Should().Be(partageId);

        dto.Nom.Should().Be(
            "Partage Seller Access Test");

        dto.NombreParticipants.Should().Be(1);
    }

    /// <summary>
    /// Un utilisateur non authentifié ne peut pas consulter la liste des partages.
    /// </summary>
    [Fact]
    public async Task GetPartages_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        var response =
            await _client.GetAsync("/api/partages");

        response.StatusCode.Should()
            .Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>
    /// Seuls les administrateurs peuvent consulter l'ensemble des partages.
    /// </summary>
    [Fact]
    public async Task GetPartages_WhenUserIsNotAdmin_ShouldReturnForbidden()
    {
        await TestAuthHelper.AuthenticateAsync(
            _client,
            TestUsers.Julien);

        var response =
            await _client.GetAsync("/api/partages");

        response.StatusCode.Should()
            .Be(HttpStatusCode.Forbidden);
    }

    /// <summary>
    /// Un administrateur peut consulter la liste complète des partages.
    /// </summary>
    [Fact]
    public async Task GetPartages_WhenUserIsAdmin_ShouldReturnOk()
    {
        await TestAuthHelper.AuthenticateAsync(
            _client,
            TestUsers.Admin);

        var response =
            await _client.GetAsync("/api/partages");

        response.StatusCode.Should()
            .Be(HttpStatusCode.OK);

        var partages =
            await response.Content
                .ReadFromJsonAsync<List<PartageSummaryDto>>();

        partages.Should().NotBeNull();
    }
}