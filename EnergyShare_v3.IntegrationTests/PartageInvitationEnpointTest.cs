using EnergyShare_v3.Application.Features.Partage;
using EnergyShare_v3.Domain.Enums;
using EnergyShare_v3.IntegrationTests.Common;
using EnergyShare_v3.Web.Models.Partage;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace EnergyShare_v3.IntegrationTests.Partage;

public class PartageInvitationEndpointTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly TestDataFactory _dataFactory;

    public PartageInvitationEndpointTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _dataFactory = new TestDataFactory(_client);
    }

    /// <summary>
    /// Vérifie que le vendeur créateur du partage peut générer
    /// un code d'invitation pour son propre partage.
    /// </summary>
    [Fact]
    public async Task GetInvitationCodePartage_WhenUserIsSeller_ShouldReturnOk()
    {
        // Création d'un vendeur avec un vrai point d'accès d'injection.
        // La méthode "Data" permet de récupérer à la fois :
        // - l'email du vendeur ;
        // - l'identifiant réel de son point d'accès.
        var seller = await _dataFactory
            .CreateSellerWithInjectionPointDataAsync();

        // Création du partage avec le vrai PointAccessId du vendeur.
        var partageId = await CreatePartageAsync(
            "Partage invitation code",
            seller.PointAccessId);

        // Le HttpClient est déjà authentifié avec le vendeur créé,
        // on peut donc demander le code d'invitation de son partage.
        var response = await _client.PostAsync(
            $"/api/partages/{partageId}/invitation-code",
            content: null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var dto = await response.Content
            .ReadFromJsonAsync<InvitationCodeDto>();

        dto.Should().NotBeNull();
        dto!.PartageId.Should().Be(partageId);
        dto.InvitationCode.Should().NotBeNullOrWhiteSpace();
        dto.InvitationCode.Should().HaveLength(12);
        dto.InvitationCodeExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    /// <summary>
    /// Vérifie qu'un utilisateur connecté mais non vendeur du partage
    /// ne peut pas générer/récupérer le code d'invitation.
    /// </summary>
    [Fact]
    public async Task GetInvitationCodePartage_WhenUserIsNotSeller_ShouldReturnForbidden()
    {
        // Création d'un vendeur avec un vrai point d'accès d'injection.
        var seller = await _dataFactory
            .CreateSellerWithInjectionPointDataAsync();

        // Création du partage avec le vrai PointAccessId du vendeur.
        var partageId = await CreatePartageAsync(
            "Partage invitation interdit",
            seller.PointAccessId);

        // Connexion avec un autre utilisateur :
        // il ne doit pas pouvoir récupérer le code d'invitation du vendeur.
        await TestAuthHelper.AuthenticateAsync(
            _client,
            TestUsers.Julien);

        var response = await _client.PostAsync(
            $"/api/partages/{partageId}/invitation-code",
            content: null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
    /// <summary>
    /// Vérifie qu'un utilisateur avec un point d'accès actif peut rejoindre
    /// un partage via un code d'invitation valide.
    ///
    /// Le vendeur et l'acheteur sont créés dynamiquement afin d'éviter
    /// les conflits avec les utilisateurs seedés.
    /// </summary>
    [Fact]
    public async Task RejoindrePartage_WithValidInvitationCode_ShouldReturnOk()
    {
        // Création du vendeur avec récupération de son vrai PointAccessId.
        var seller = await _dataFactory
            .CreateSellerWithInjectionPointDataAsync();

        // Création du partage avec le vrai point d'injection du vendeur.
        var partageId = await CreatePartageAsync(
            "Partage à rejoindre",
            seller.PointAccessId);

        // Génération du code d'invitation.
        var invitationCode = await CreateInvitationCodeAsync(partageId);

        // Création de l'acheteur avec son point de consommation.
        // Pour le moment, on conserve l'ancienne logique de rejoindre un partage.
        await _dataFactory.CreateBuyerWithConsumptionPointAsync();

        var response = await _client.PostAsJsonAsync(
            "/api/partages/rejoindre",
            new RejoindrePartageRequest(invitationCode));

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var joinedPartageId = await response.Content
            .ReadFromJsonAsync<Guid>();

        joinedPartageId.Should().Be(partageId);
    }

    /// <summary>
    /// Vérifie qu'un code d'invitation inconnu est refusé.
    /// </summary>
    [Fact]
    public async Task RejoindrePartage_WithInvalidCode_ShouldReturnBadRequest()
    {
        await _dataFactory.CreateBuyerWithConsumptionPointAsync();

        var response = await _client.PostAsJsonAsync(
            "/api/partages/rejoindre",
            new RejoindrePartageRequest("CODEINCONNU"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Vérifie qu'un utilisateur non authentifié ne peut pas rejoindre
    /// un partage, même s'il fournit un code.
    /// </summary>
    [Fact]
    public async Task RejoindrePartage_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var response = await _client.PostAsJsonAsync(
            "/api/partages/rejoindre",
            new RejoindrePartageRequest("ABC123456789"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>
    /// Helper local : crée un partage avec l'utilisateur actuellement authentifié.
    /// </summary>
    /// <summary>
    /// Helper local : crée un partage avec l'utilisateur actuellement authentifié.
    /// Le PointAccessId doit correspondre à un vrai point d'accès
    /// actif et injecteur appartenant à cet utilisateur.
    /// </summary>
    private async Task<Guid> CreatePartageAsync(
        string nom,
        Guid pointAccessId)
    {
        var createResponse = await _client.PostAsJsonAsync(
            "/api/partages",
            new CreatePartage(
                Nom: nom,
                EnergieType: PartageEnergieType.PairToPair,
                PointAccessId: pointAccessId));

        var body = await createResponse.Content.ReadAsStringAsync();

        createResponse.StatusCode.Should().Be(
            HttpStatusCode.Created,
            $"Création du partage échouée. Réponse API : {body}");

        var partageId = await createResponse.Content
            .ReadFromJsonAsync<Guid>();

        partageId.Should().NotBeEmpty();

        return partageId;
    }

    /// <summary>
    /// Helper local : génère un code d'invitation pour le partage donné.
    /// L'utilisateur actuellement authentifié doit être le vendeur du partage.
    /// </summary>
    private async Task<string> CreateInvitationCodeAsync(Guid partageId)
    {
        var invitationResponse = await _client.PostAsync(
            $"/api/partages/{partageId}/invitation-code",
            content: null);

        invitationResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var invitationDto = await invitationResponse.Content
            .ReadFromJsonAsync<InvitationCodeDto>();

        invitationDto.Should().NotBeNull();
        invitationDto!.InvitationCode.Should().NotBeNullOrWhiteSpace();

        return invitationDto.InvitationCode;
    }
}