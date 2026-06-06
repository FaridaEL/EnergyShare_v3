using EnergyShare_v3.Application.Features.Partage;
using EnergyShare_v3.Domain.Enums;
using EnergyShare_v3.IntegrationTests.Common;
using EnergyShare_v3.Web.Models.Partage;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace EnergyShare_v3.IntegrationTests.Partage;

public class PartageValidationEndpointTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly TestDataFactory _dataFactory;

    public PartageValidationEndpointTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _dataFactory = new TestDataFactory(_client);
    }



    //Validation d'un partage complet 
    [Fact]
    public async Task DemandeValidationPartage_WhenSellerAndPartageReady_ShouldReturnOk()
    {
        // Arrange
        var sellerEmail =
            await _dataFactory.CreateSellerWithInjectionPointAsync();

        await TestAuthHelper.AuthenticateAsync(_client, sellerEmail);

        var createResponse = await _client.PostAsJsonAsync(
            "/api/partages",
            new CreatePartage(
                "Partage Validation Test",
                PartageEnergieType.MemeBatiment));

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var partageId =
            await createResponse.Content.ReadFromJsonAsync<Guid>();


        var buyerEmail =
            await _dataFactory.CreateBuyerWithConsumptionPointAsync();

        await TestAuthHelper.AuthenticateAsync(_client, buyerEmail);

        var invitationResponse =
            await _client.PostAsync(
                $"/api/partages/{partageId}/invitation-code",
                null);

        invitationResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        // retour vendeur
        await TestAuthHelper.AuthenticateAsync(_client, sellerEmail);

        invitationResponse =
            await _client.PostAsync(
                $"/api/partages/{partageId}/invitation-code",
                null);

        invitationResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var invitation =
            await invitationResponse.Content
                .ReadFromJsonAsync<InvitationCodeDto>();

        await TestAuthHelper.AuthenticateAsync(_client, buyerEmail);

        var joinResponse = await _client.PostAsJsonAsync(
            "/api/partages/rejoindre",
            new RejoindrePartageRequest(
                invitation!.InvitationCode));

        joinResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act
        await TestAuthHelper.AuthenticateAsync(_client, sellerEmail);

        var response = await _client.PostAsync(
            $"/api/partages/{partageId}/demande-validation",
            null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var dto =
            await response.Content
                .ReadFromJsonAsync<DemandeValidationPartageDto>();

        dto.Should().NotBeNull();

        dto!.PartageId.Should().Be(partageId);
        dto.DemandeId.Should().NotBeEmpty();
        dto.ResponseStatus.Should()
            .Be(DdeGRDResponseStatus.EnAttente.ToString());
    }


    //Validation refusée car pas de périmètre
    [Fact]
    public async Task DemandeValidationPartage_WhenPairToPairWithoutPerimetre_ShouldReturnBadRequest()
    {
        var sellerEmail =
            await _dataFactory.CreateSellerWithInjectionPointAsync();

        var buyerEmail =
            await _dataFactory.CreateBuyerWithConsumptionPointAsync();

        await TestAuthHelper.AuthenticateAsync(_client, sellerEmail);

        var createResponse = await _client.PostAsJsonAsync(
            "/api/partages",
            new CreatePartage(
                "Partage Sans Périmètre",
                PartageEnergieType.PairToPair));

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var partageId =
            await createResponse.Content.ReadFromJsonAsync<Guid>();


        var invitationResponse =
            await _client.PostAsync(
                $"/api/partages/{partageId}/invitation-code",
                null);

        var invitation =
            await invitationResponse.Content
                .ReadFromJsonAsync<InvitationCodeDto>();


        await TestAuthHelper.AuthenticateAsync(_client, buyerEmail);

        await _client.PostAsJsonAsync(
            "/api/partages/rejoindre",
            new RejoindrePartageRequest(
                invitation!.InvitationCode));

        await TestAuthHelper.AuthenticateAsync(_client, sellerEmail);

        var response = await _client.PostAsync(
            $"/api/partages/{partageId}/demande-validation",
            null);

        response.StatusCode.Should()
            .Be(HttpStatusCode.BadRequest);
    }


    //Utilsateur non vendeur tente de demander validation
    [Fact]
    public async Task DemandeValidationPartage_WhenUserIsNotSeller_ShouldReturnForbidden()
    {
        var sellerEmail =
            await _dataFactory.CreateSellerWithInjectionPointAsync();

        await TestAuthHelper.AuthenticateAsync(_client, sellerEmail);

        var createResponse = await _client.PostAsJsonAsync(
            "/api/partages",
            new CreatePartage(
                "Partage sécurisé",
                PartageEnergieType.MemeBatiment));

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var partageId =
            await createResponse.Content.ReadFromJsonAsync<Guid>();


        var otherUser =
            await _dataFactory.CreateBuyerWithConsumptionPointAsync();

        await TestAuthHelper.AuthenticateAsync(_client, otherUser);

        var response = await _client.PostAsync(
            $"/api/partages/{partageId}/demande-validation",
            null);

        response.StatusCode.Should()
            .Be(HttpStatusCode.Forbidden);
    }

    //Validation déjà en attente 
    [Fact]
    public async Task DemandeValidationPartage_WhenValidationAlreadyPending_ShouldReturnBadRequest()
    {
        var sellerEmail =
            await _dataFactory.CreateSellerWithInjectionPointAsync();

        var buyerEmail =
            await _dataFactory.CreateBuyerWithConsumptionPointAsync();

        await TestAuthHelper.AuthenticateAsync(_client, sellerEmail);

        var createResponse = await _client.PostAsJsonAsync(
            "/api/partages",
            new CreatePartage(
                "Partage doublon validation",
                PartageEnergieType.MemeBatiment));

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var partageId =
            await createResponse.Content.ReadFromJsonAsync<Guid>();


        var invitationResponse =
            await _client.PostAsync(
                $"/api/partages/{partageId}/invitation-code",
                null);

        var invitation =
            await invitationResponse.Content
                .ReadFromJsonAsync<InvitationCodeDto>();


        await TestAuthHelper.AuthenticateAsync(_client, buyerEmail);

        await _client.PostAsJsonAsync(
            "/api/partages/rejoindre",
            new RejoindrePartageRequest(
                invitation!.InvitationCode));

        await TestAuthHelper.AuthenticateAsync(_client, sellerEmail);

        var firstResponse = await _client.PostAsync(
            $"/api/partages/{partageId}/demande-validation",
            null);

        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var secondResponse = await _client.PostAsync(
            $"/api/partages/{partageId}/demande-validation",
            null);

        secondResponse.StatusCode.Should()
            .Be(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Vérifie le scénario complet :
    /// vendeur crée un partage même bâtiment,
    /// acheteur rejoint,
    /// vendeur demande validation,
    /// GRD valide,
    /// partage devient actif.
    /// </summary>
    [Fact]
    public async Task RepondreDemandeValidationPartage_WhenGrdValidates_ShouldReturnOk()
    {
        var demandeId = await CreateValidationRequestForMemeBatimentAsync(
            "Partage validation GRD");

        await TestAuthHelper.AuthenticateAsync(
            _client,
            TestUsers.AgentSibelga);

        var response = await _client.PostAsJsonAsync(
            $"/api/partages/demandes-grd/{demandeId}/validation/repondre",
            new RepondreDemandeValidationPartageRequest(
                true,
                "Partage validé par le GRD."));

        var body = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.OK,
            $"Réponse API : {body}");

        var dto = await response.Content
            .ReadFromJsonAsync<ReponseDemandeValidationPartageDto>();

        dto.Should().NotBeNull();
        dto!.ResponseStatus.Should().Be("Valide");
        dto.StatutPartage.Should().Be(PartageEnergieStatutType.Actif);
        dto.CommentaireReponseGRD.Should().Be("Partage validé par le GRD.");
    }

    /// <summary>
    /// Vérifie qu'un utilisateur standard ne peut pas répondre à une dde de validation GRD.
    /// </summary>
    [Fact]
    public async Task RepondreDemandeValidationPartage_WhenUserIsNotGrd_ShouldReturnForbidden()
    {
        await TestAuthHelper.AuthenticateAsync(
            _client,
            TestUsers.Julien);

        var response = await _client.PostAsJsonAsync(
            $"/api/partages/demandes-grd/{Guid.NewGuid()}/validation/repondre",
            new RepondreDemandeValidationPartageRequest(
                true,
                "Tentative interdite."));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    /// <summary>
    /// Vérifie qu'un GRD peut refuser une dde de validation
    /// et que le partage revient/reste en statut Inactif.
    /// </summary>
    [Fact]
    public async Task RepondreDemandeValidationPartage_WhenGrdRefuses_ShouldReturnOk()
    {
        var demandeId = await CreateValidationRequestForMemeBatimentAsync(
            "Partage refusé");

        await TestAuthHelper.AuthenticateAsync(
            _client,
            TestUsers.AgentSibelga);

        var response = await _client.PostAsJsonAsync(
            $"/api/partages/demandes-grd/{demandeId}/validation/repondre",
            new RepondreDemandeValidationPartageRequest(
                false,
                "Le dossier est incomplet."));
         
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK,
             $"Réponse API : {body}");

        var dto = await response.Content
            .ReadFromJsonAsync<ReponseDemandeValidationPartageDto>();

        dto.Should().NotBeNull();
        dto!.ResponseStatus.Should().Be("Refus");
        dto.StatutPartage.Should().Be(PartageEnergieStatutType.Inactif);
        dto.CommentaireReponseGRD.Should().Be("Le dossier est incomplet.");
    }

    /// <summary>
    /// Crée un partage "Même bâtiment" complet et introduit
    /// une demande de validation GRD.
    ///
    /// Le type MêmeBatiment évite ici la demande préalable de périmètre,
    /// car le périmètre A est défini automatiquement par la logique métier.
    /// </summary>
    private async Task<Guid> CreateValidationRequestForMemeBatimentAsync(string nomPartage)
    {
        var sellerEmail = await _dataFactory.CreateSellerWithInjectionPointAsync();

        var partageId = await CreatePartageAsync(
            nomPartage,
            PartageEnergieType.MemeBatiment);

        var invitationCode = await CreateInvitationCodeAsync(partageId);

        await _dataFactory.CreateBuyerWithConsumptionPointAsync();

        var joinResponse = await _client.PostAsJsonAsync(
            "/api/partages/rejoindre",
            new RejoindrePartageRequest(invitationCode));

        var joinBody = await joinResponse.Content.ReadAsStringAsync();

        joinResponse.StatusCode.Should().Be(
            HttpStatusCode.OK,
            $"Rejoindre partage échoué. Réponse API : {joinBody}");

        await TestAuthHelper.AuthenticateAsync(
            _client,
            sellerEmail);

        var demandeResponse = await _client.PostAsync(
            $"/api/partages/{partageId}/demande-validation",
            null);

        var demandeBody = await demandeResponse.Content.ReadAsStringAsync();

        demandeResponse.StatusCode.Should().Be(
            HttpStatusCode.OK,
            $"Demande de validation échouée. Réponse API : {demandeBody}");

        var demandeDto = await demandeResponse.Content
            .ReadFromJsonAsync<DemandeValidationPartageDto>();

        demandeDto.Should().NotBeNull();
        demandeDto!.DemandeId.Should().NotBeEmpty();

        return demandeDto.DemandeId;
    }

    /// <summary>
    /// Crée un partage avec l'utilisateur actuellement authentifié.
    /// </summary>
    private async Task<Guid> CreatePartageAsync(
        string nom,
        PartageEnergieType energieType)
    {
        var createResponse = await _client.PostAsJsonAsync(
            "/api/partages",
            new CreatePartage(
                Nom: nom,
                EnergieType: energieType));

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
    /// Génère un code d'invitation pour un partage.
    /// L'utilisateur actuellement authentifié doit être le vendeur.
    /// </summary>
    private async Task<string> CreateInvitationCodeAsync(Guid partageId)
    {
        var invitationResponse = await _client.PostAsync(
            $"/api/partages/{partageId}/invitation-code",
            null);

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