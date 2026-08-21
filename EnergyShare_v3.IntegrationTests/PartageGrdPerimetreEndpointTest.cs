using EnergyShare_v3.Application.Features.Partage;
using EnergyShare_v3.Domain.Enums;
using EnergyShare_v3.IntegrationTests.Common;
using EnergyShare_v3.Web.Models.Partage;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace EnergyShare_v3.IntegrationTests.Partage;

public class PartageGrdPerimetreEndpointTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly TestDataFactory _dataFactory;

    public PartageGrdPerimetreEndpointTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _dataFactory = new TestDataFactory(_client);
    }

    /// <summary>
    /// Vérifie qu'un vendeur peut demander les informations de périmètre
    /// lorsque son partage est complet, c'est-à-dire avec au moins deux participants.
    /// </summary>
    [Fact]
    public async Task DemandeInfoPerimetrePartage_WhenSellerAndPartageComplete_ShouldReturnOk()
    {
        var partageId = await CreateCompletePartageAsync("Partage demande périmètre");

        var response = await _client.PostAsync(
            $"/api/partages/{partageId}/demande-info-perimetre",
            null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var dto = await response.Content.ReadFromJsonAsync<DemandePerimetreDto>();

        dto.Should().NotBeNull();
        dto!.PartageId.Should().Be(partageId);
        dto.DemandeId.Should().NotBeEmpty();
        dto.ResponseStatus.Should().Be(DdeGRDResponseStatus.EnAttente.ToString());
        dto.DetailsDemande.Should().Contain("Adresses des points d’accès concernés");
    }

    /// <summary>
    /// Vérifie qu'un organisme public / GRD peut consulter
    /// les demandes GRD en attente.
    /// </summary>
    [Fact]
    public async Task GetDemandesGrdEnAttente_WhenUserIsOrganismePublic_ShouldReturnOk()
    {
        await TestAuthHelper.AuthenticateAsync(
            _client,
            TestUsers.AgentSibelga);

        var response = await _client.GetAsync(
            "/api/partages/demandes-grd/en-attente");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var demandes = await response.Content
            .ReadFromJsonAsync<List<DemandeGrdDto>>();

        demandes.Should().NotBeNull();
    }

    /// <summary>
    /// Vérifie qu'un utilisateur standard ne peut pas consulter
    /// les demandes GRD en attente.
    /// </summary>
    [Fact]
    public async Task GetDemandesGrdEnAttente_WhenUserIsNotAdminOrOrganismePublic_ShouldReturnForbidden()
    {
        await TestAuthHelper.AuthenticateAsync(
            _client,
            TestUsers.Julien);

        var response = await _client.GetAsync(
            "/api/partages/demandes-grd/en-attente");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    /// <summary>
    /// Vérifie qu'un utilisateur non authentifié ne peut pas consulter
    /// les demandes GRD en attente.
    /// </summary>
    [Fact]
    public async Task GetDemandesGrdEnAttente_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var response = await _client.GetAsync(
            "/api/partages/demandes-grd/en-attente");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>
    /// Vérifie qu'une demande de périmètre créée par un vendeur apparaît
    /// bien dans la liste des demandes GRD en attente.
    /// </summary>
    [Fact]
    public async Task GetDemandesGrdEnAttente_WhenDemandeExists_ShouldReturnDemande()
    {
        var partageId = await CreateCompletePartageAsync("Partage demande GRD visible");

        var demandeResponse = await _client.PostAsync(
            $"/api/partages/{partageId}/demande-info-perimetre",
            null);

        demandeResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var demandeDto = await demandeResponse.Content
            .ReadFromJsonAsync<DemandePerimetreDto>();

        demandeDto.Should().NotBeNull();

        await TestAuthHelper.AuthenticateAsync(
            _client,
            TestUsers.Admin);

        var response = await _client.GetAsync(
            "/api/partages/demandes-grd/en-attente");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var demandes = await response.Content
            .ReadFromJsonAsync<List<DemandeGrdDto>>();

        demandes.Should().NotBeNull();

        demandes.Should().Contain(d =>
            d.Id == demandeDto!.DemandeId &&
            d.PartageId == partageId &&
            d.ResponseStatus == DdeGRDResponseStatus.EnAttente &&
            d.DemandeType == DemandeGRDType.DdeInfoPerimetre);
    }

    /// <summary>
    /// Vérifie qu'un agent GRD peut répondre à une demande de périmètre
    /// et confirmer le périmètre applicable.
    /// </summary>
    [Fact]
    public async Task RepondreDemandePerimetre_WhenAgentGrd_ShouldReturnOk()
    {
        var partageId = await CreateCompletePartageAsync("Test GRD réponse");

        var demandeResponse = await _client.PostAsync(
            $"/api/partages/{partageId}/demande-info-perimetre",
            null);

        demandeResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var demande = await demandeResponse.Content
            .ReadFromJsonAsync<DemandePerimetreDto>();

        demande.Should().NotBeNull();

        await TestAuthHelper.AuthenticateAsync(
            _client,
            TestUsers.AgentSibelga);

        var response = await _client.PostAsJsonAsync(
            $"/api/partages/demandes-grd/{demande!.DemandeId}/repondre",
            new RepondreDemandePerimetreRequest(
                PerimetreType.A,
                "Périmètre A confirmé par le GRD"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var dto = await response.Content
            .ReadFromJsonAsync<ReponseDemandePerimetreDto>();

        dto.Should().NotBeNull();
        dto!.PerimetreConfirme.Should().Be(PerimetreType.A);
        dto.CommentaireReponseGRD.Should().Be("Périmètre A confirmé par le GRD");
        dto.ResponseStatus.Should().Be(DdeGRDResponseStatus.Valide.ToString());
    }

    /// <summary>
    /// Vérifie qu'un utilisateur standard ne peut pas répondre
    /// à une demande de périmètre à la place du GRD.
    /// </summary>
    [Fact]
    public async Task RepondreDemandePerimetre_WhenUserNotGrd_ShouldReturnForbidden()
    {
        await TestAuthHelper.AuthenticateAsync(
            _client,
            TestUsers.Julien);

        var response = await _client.PostAsJsonAsync(
            $"/api/partages/demandes-grd/{Guid.NewGuid()}/repondre",
            new RepondreDemandePerimetreRequest(
                PerimetreType.A,
                null));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    /// <summary>
    /// Crée un partage complet :
    /// - vendeur dynamique avec point d'injection ;
    /// - création du partage ;
    /// - génération du code d'invitation ;
    /// - acheteur dynamique avec point de consommation ;
    /// - acheteur rejoint le partage.
    ///
    /// À la fin de cette méthode, le HttpClient est ré-authentifié
    /// avec le vendeur, car c'est lui qui doit effectuer les actions suivantes
    /// sur le partage.
    /// </summary>
    /// <summary>
    /// Crée un partage complet pour les tests d'intégration :
    /// - création d'un vendeur avec un point d'accès injecteur actif ;
    /// - création du partage avec le vrai PointAccessId du vendeur ;
    /// - génération d'un code d'invitation ;
    /// - création d'un acheteur avec un point de consommation ;
    /// - l'acheteur rejoint le partage.
    ///
    /// À la fin, le HttpClient est ré-authentifié avec le vendeur
    /// afin que les actions suivantes soient réalisées avec le bon utilisateur.
    /// </summary>
    private async Task<Guid> CreateCompletePartageAsync(string nom)
    {
        // 1. Création du vendeur.
        // On utilise la méthode "Data" car CreatePartage exige maintenant
        // le vrai PointAccessId du point injecteur sélectionné.
        var seller = await _dataFactory
            .CreateSellerWithInjectionPointDataAsync();

        // 2. Création du partage avec le vrai point d'accès du vendeur.
        var partageId = await CreatePartageAsync(
            nom,
            seller.PointAccessId);

        // 3. Génération du code d'invitation.
        var invitationCode = await CreateInvitationCodeAsync(partageId);

        // 4. Création d'un acheteur avec un point de consommation.
        // Pour le moment, on conserve l'ancienne logique de rejoindre un partage.
        await _dataFactory.CreateBuyerWithConsumptionPointAsync();

        // 5. L'acheteur rejoint le partage avec le code d'invitation.
        var joinResponse = await _client.PostAsJsonAsync(
            "/api/partages/rejoindre",
            new RejoindrePartageRequest(invitationCode));

        var joinBody = await joinResponse.Content.ReadAsStringAsync();

        joinResponse.StatusCode.Should().Be(
            HttpStatusCode.OK,
            $"Rejoindre partage échoué. Réponse API : {joinBody}");

        // 6. On se ré-authentifie avec le vendeur pour poursuivre le test.
        await TestAuthHelper.AuthenticateAsync(
            _client,
            seller.Email);

        return partageId;
    }

    /// <summary>
    /// Crée un partage avec l'utilisateur actuellement authentifié.
    /// Le PointAccessId reçu doit correspondre à un vrai point d'accès
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
    /// Génère un code d'invitation pour le partage donné.
    /// L'utilisateur actuellement authentifié doit être le vendeur du partage.
    /// </summary>
    private async Task<string> CreateInvitationCodeAsync(Guid partageId)
    {
        var invitationResponse = await _client.PostAsync(
            $"/api/partages/{partageId}/invitation-code",
            content: null);

        var body = await invitationResponse.Content.ReadAsStringAsync();

        invitationResponse.StatusCode.Should().Be(
            HttpStatusCode.OK,
            $"Création du code d'invitation échouée. Réponse API : {body}");

        var invitationDto = await invitationResponse.Content
            .ReadFromJsonAsync<InvitationCodeDto>();

        invitationDto.Should().NotBeNull();
        invitationDto!.InvitationCode.Should().NotBeNullOrWhiteSpace();

        return invitationDto.InvitationCode;
    }
}