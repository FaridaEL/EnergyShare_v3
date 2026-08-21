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



    // Validation d'un partage complet
    [Fact]
    public async Task DemandeValidationPartage_WhenSellerAndPartageReady_ShouldReturnOk()
    {
        // =========================================================
        // 1. CRÉATION DU VENDEUR AVEC SON VRAI POINT D'INJECTION
        // =========================================================

        var seller = await _dataFactory
            .CreateSellerWithInjectionPointDataAsync();

        await TestAuthHelper.AuthenticateAsync(
            _client,
            seller.Email);


        // =========================================================
        // 2. CRÉATION DU PARTAGE AVEC LE VRAI POINTACCESSID
        // =========================================================

        var createResponse = await _client.PostAsJsonAsync(
            "/api/partages",
            new CreatePartage(
                "Partage Validation Test",
                PartageEnergieType.MemeBatiment,
                seller.PointAccessId));

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var partageId =
            await createResponse.Content.ReadFromJsonAsync<Guid>();

        partageId.Should().NotBeEmpty();


        // =========================================================
        // 3. CRÉATION DE L'ACHETEUR
        // =========================================================

        var buyerEmail =
            await _dataFactory.CreateBuyerWithConsumptionPointAsync();

        await TestAuthHelper.AuthenticateAsync(
            _client,
            buyerEmail);


        // =========================================================
        // 4. L'ACHETEUR NE PEUT PAS GÉNÉRER LE CODE D'INVITATION
        // =========================================================

        var invitationResponse =
            await _client.PostAsync(
                $"/api/partages/{partageId}/invitation-code",
                null);

        invitationResponse.StatusCode.Should().Be(
            HttpStatusCode.Forbidden);


        // =========================================================
        // 5. RETOUR VENDEUR POUR GÉNÉRER LE CODE
        // =========================================================

        await TestAuthHelper.AuthenticateAsync(
            _client,
            seller.Email);

        invitationResponse =
            await _client.PostAsync(
                $"/api/partages/{partageId}/invitation-code",
                null);

        invitationResponse.StatusCode.Should().Be(
            HttpStatusCode.OK);

        var invitation =
            await invitationResponse.Content
                .ReadFromJsonAsync<InvitationCodeDto>();

        invitation.Should().NotBeNull();


        // =========================================================
        // 6. L'ACHETEUR REJOINT LE PARTAGE
        // =========================================================

        await TestAuthHelper.AuthenticateAsync(
            _client,
            buyerEmail);

        var joinResponse = await _client.PostAsJsonAsync(
            "/api/partages/rejoindre",
            new RejoindrePartageRequest(
                invitation!.InvitationCode));

        joinResponse.StatusCode.Should().Be(
            HttpStatusCode.OK);


        // =========================================================
        // 7. LE VENDEUR DEMANDE LA VALIDATION GRD
        // =========================================================

        await TestAuthHelper.AuthenticateAsync(
            _client,
            seller.Email);

        var response = await _client.PostAsync(
            $"/api/partages/{partageId}/demande-validation",
            null);


        // =========================================================
        // 8. VÉRIFICATIONS
        // =========================================================

        response.StatusCode.Should().Be(
            HttpStatusCode.OK);

        var dto =
            await response.Content
                .ReadFromJsonAsync<DemandeValidationPartageDto>();

        dto.Should().NotBeNull();

        dto!.PartageId.Should().Be(partageId);
        dto.DemandeId.Should().NotBeEmpty();
        dto.ResponseStatus.Should()
            .Be(DdeGRDResponseStatus.EnAttente.ToString());
    }

    // Validation refusée car pas de périmètre
    [Fact]
    public async Task DemandeValidationPartage_WhenPairToPairWithoutPerimetre_ShouldReturnBadRequest()
    {
        // Création du vendeur avec récupération de son vrai PointAccessId.
        var seller = await _dataFactory
            .CreateSellerWithInjectionPointDataAsync();

        // Création de l'acheteur.
        var buyerEmail =
            await _dataFactory.CreateBuyerWithConsumptionPointAsync();

        // Retour sur le vendeur pour créer le partage.
        await TestAuthHelper.AuthenticateAsync(
            _client,
            seller.Email);

        var createResponse = await _client.PostAsJsonAsync(
            "/api/partages",
            new CreatePartage(
                "Partage Sans Périmètre",
                PartageEnergieType.PairToPair,
                seller.PointAccessId));

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var partageId =
            await createResponse.Content.ReadFromJsonAsync<Guid>();

        // Le vendeur génère le code d'invitation.
        var invitationResponse =
            await _client.PostAsync(
                $"/api/partages/{partageId}/invitation-code",
                null);

        var invitation =
            await invitationResponse.Content
                .ReadFromJsonAsync<InvitationCodeDto>();

        // L'acheteur rejoint le partage.
        await TestAuthHelper.AuthenticateAsync(
            _client,
            buyerEmail);

        await _client.PostAsJsonAsync(
            "/api/partages/rejoindre",
            new RejoindrePartageRequest(
                invitation!.InvitationCode));

        // Retour vendeur pour demander la validation.
        await TestAuthHelper.AuthenticateAsync(
            _client,
            seller.Email);

        var response = await _client.PostAsync(
            $"/api/partages/{partageId}/demande-validation",
            null);

        // PairToPair sans périmètre confirmé => refus.
        response.StatusCode.Should()
            .Be(HttpStatusCode.BadRequest);
    }


    // Utilisateur non vendeur tente de demander validation
    [Fact]
    public async Task DemandeValidationPartage_WhenUserIsNotSeller_ShouldReturnForbidden()
    {
        // Création du vrai vendeur + récupération de son PointAccessId.
        var seller = await _dataFactory
            .CreateSellerWithInjectionPointDataAsync();

        await TestAuthHelper.AuthenticateAsync(
            _client,
            seller.Email);

        // Le partage est créé avec le vrai point injecteur du vendeur.
        var createResponse = await _client.PostAsJsonAsync(
            "/api/partages",
            new CreatePartage(
                "Partage sécurisé",
                PartageEnergieType.MemeBatiment,
                seller.PointAccessId));

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var partageId =
            await createResponse.Content.ReadFromJsonAsync<Guid>();

        // Création d'un autre utilisateur.
        var otherUser =
            await _dataFactory.CreateBuyerWithConsumptionPointAsync();

        await TestAuthHelper.AuthenticateAsync(
            _client,
            otherUser);

        // Cet utilisateur ne doit pas pouvoir demander
        // la validation du partage du vendeur.
        var response = await _client.PostAsync(
            $"/api/partages/{partageId}/demande-validation",
            null);

        response.StatusCode.Should()
            .Be(HttpStatusCode.Forbidden);
    }

    // Validation déjà en attente
    [Fact]
    public async Task DemandeValidationPartage_WhenValidationAlreadyPending_ShouldReturnBadRequest()
    {
        // Création du vendeur avec récupération du vrai PointAccessId.
        var seller = await _dataFactory
            .CreateSellerWithInjectionPointDataAsync();

        // Création de l'acheteur.
        var buyerEmail =
            await _dataFactory.CreateBuyerWithConsumptionPointAsync();

        // Retour vendeur.
        await TestAuthHelper.AuthenticateAsync(
            _client,
            seller.Email);

        // Création du partage avec le vrai point injecteur.
        var createResponse = await _client.PostAsJsonAsync(
            "/api/partages",
            new CreatePartage(
                "Partage doublon validation",
                PartageEnergieType.MemeBatiment,
                seller.PointAccessId));

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var partageId =
            await createResponse.Content.ReadFromJsonAsync<Guid>();

        // Génération du code d'invitation.
        var invitationResponse =
            await _client.PostAsync(
                $"/api/partages/{partageId}/invitation-code",
                null);

        var invitation =
            await invitationResponse.Content
                .ReadFromJsonAsync<InvitationCodeDto>();

        // L'acheteur rejoint le partage.
        await TestAuthHelper.AuthenticateAsync(
            _client,
            buyerEmail);

        await _client.PostAsJsonAsync(
            "/api/partages/rejoindre",
            new RejoindrePartageRequest(
                invitation!.InvitationCode));

        // Retour vendeur.
        await TestAuthHelper.AuthenticateAsync(
            _client,
            seller.Email);

        // Première demande : acceptée.
        var firstResponse = await _client.PostAsync(
            $"/api/partages/{partageId}/demande-validation",
            null);

        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Deuxième demande : refusée car une demande
        // est déjà en attente.
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
    /// Crée un partage "Même bâtiment" complet puis introduit
    /// une demande de validation GRD.
    ///
    /// Le scénario réalisé est le suivant :
    /// - création d'un vendeur avec un vrai point d'accès injecteur ;
    /// - création du partage avec ce PointAccessId ;
    /// - génération d'un code d'invitation ;
    /// - création d'un acheteur avec un point de consommation ;
    /// - l'acheteur rejoint le partage ;
    /// - retour sur le vendeur ;
    /// - introduction de la demande de validation GRD.
    ///
    /// La méthode retourne l'identifiant de la demande GRD créée.
    /// </summary>
    private async Task<Guid> CreateValidationRequestForMemeBatimentAsync(
        string nomPartage)
    {
        // 1. Création du vendeur avec récupération :
        // - de son email ;
        // - du vrai PointAccessId de son point injecteur.
        var seller = await _dataFactory
            .CreateSellerWithInjectionPointDataAsync();

        // 2. Création du partage avec le vrai point d'accès du vendeur.
        var partageId = await CreatePartageAsync(
            nomPartage,
            PartageEnergieType.MemeBatiment,
            seller.PointAccessId);

        // 3. Génération du code d'invitation.
        var invitationCode =
            await CreateInvitationCodeAsync(partageId);

        // 4. Création d'un acheteur avec un point de consommation.
        // La méthode laisse le HttpClient authentifié avec cet acheteur.
        await _dataFactory
            .CreateBuyerWithConsumptionPointAsync();

        // 5. L'acheteur rejoint le partage.
        var joinResponse = await _client.PostAsJsonAsync(
            "/api/partages/rejoindre",
            new RejoindrePartageRequest(
                invitationCode));

        var joinBody =
            await joinResponse.Content.ReadAsStringAsync();

        joinResponse.StatusCode.Should().Be(
            HttpStatusCode.OK,
            $"Rejoindre partage échoué. Réponse API : {joinBody}");

        // 6. Retour sur le vendeur.
        await TestAuthHelper.AuthenticateAsync(
            _client,
            seller.Email);

        // 7. Le vendeur introduit la demande de validation GRD.
        var demandeResponse = await _client.PostAsync(
            $"/api/partages/{partageId}/demande-validation",
            null);

        var demandeBody =
            await demandeResponse.Content.ReadAsStringAsync();

        demandeResponse.StatusCode.Should().Be(
            HttpStatusCode.OK,
            $"Demande de validation échouée. Réponse API : {demandeBody}");

        // 8. Lecture et vérification du DTO retourné.
        var demandeDto = await demandeResponse.Content
            .ReadFromJsonAsync<DemandeValidationPartageDto>();

        demandeDto.Should().NotBeNull();
        demandeDto!.DemandeId.Should().NotBeEmpty();

        return demandeDto.DemandeId;
    }

    /// <summary>
    /// Crée un partage avec l'utilisateur actuellement authentifié.
    /// Le PointAccessId doit correspondre à un vrai point d'accès
    /// actif et injecteur appartenant à cet utilisateur.
    /// </summary>
    private async Task<Guid> CreatePartageAsync(
        string nom,
        PartageEnergieType energieType,
        Guid pointAccessId)
    {
        var createResponse = await _client.PostAsJsonAsync(
            "/api/partages",
            new CreatePartage(
                Nom: nom,
                EnergieType: energieType,
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