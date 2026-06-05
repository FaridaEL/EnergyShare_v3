using EnergyShare_v3.Application.Features.Partage;
using EnergyShare_v3.Domain.Enums;
using EnergyShare_v3.Web.Models.Partage;
using FluentAssertions;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using static EnergyShare_v3.Web.Endpoints.PartageEndpoint;

namespace EnergyShare_v3.IntegrationTests
{
    public class PartageEndpointTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private static string? _sarahToken;    //Vendeur
        private static string? _julienToken;  //Vendeur
        private static string? _adminToken;
        private static string? _leaToken;   //Acheteur
        private static string? _sibelgaToken; //GRD
        private static string? _hugoToken;//acheteur
        private static string? _boulangerieToken; // acheteur

        public PartageEndpointTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

     

        ////Gestion dde validation d'un nouveau partage par le GRD
        //// Validation d'un nouveau partage par le vendeur
        //[Fact]
        //public async Task DemandeValidationPartage_WhenSellerAndPartageReady_ShouldReturnOk()
        //{
        //    // Arrange : Sarah crée un partage pair-à-pair.
        //    await AuthenticateAsync("sarah.dupont@example.com");

        //    var createResponse = await _client.PostAsJsonAsync(
        //        "/api/partages",
        //        new CreatePartage("Partage validation nouveau", PartageEnergieType.PairToPair));

        //    createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        //    var partageId = await createResponse.Content.ReadFromJsonAsync<Guid>();

        //    // Sarah invite un participant.
        //    var invitationResponse = await _client.PostAsync(
        //        $"/api/partages/{partageId}/invitation-code",
        //        null);

        //    invitationResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        //    var invitationDto = await invitationResponse.Content.ReadFromJsonAsync<InvitationCodeDto>();
        //    invitationDto.Should().NotBeNull();

        //    // Boulangerie rejoint le partage.
        //    await AuthenticateAsync("contact@boulangerie-dupain.be");

        //    var joinResponse = await _client.PostAsJsonAsync(
        //        "/api/partages/rejoindre",
        //        new RejoindrePartageRequest(invitationDto!.InvitationCode));

        //    joinResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        //    // Sarah demande d'abord les infos de périmètre.
        //    await AuthenticateAsync("sarah.dupont@example.com");

        //    var demandePerimetreResponse = await _client.PostAsync(
        //        $"/api/partages/{partageId}/demande-info-perimetre",
        //        null);

        //    demandePerimetreResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        //    var demandePerimetreDto =
        //        await demandePerimetreResponse.Content.ReadFromJsonAsync<DemandePerimetreDto>();

        //    demandePerimetreDto.Should().NotBeNull();

        //    // Le GRD confirme le périmètre.
        //    await AuthenticateAsync("agent.sibelga@example.com");

        //    var reponsePerimetreResponse = await _client.PostAsJsonAsync(
        //        $"/api/partages/demandes-grd/{demandePerimetreDto!.DemandeId}/repondre",
        //        new RepondreDemandePerimetreRequest(
        //            PerimetreType.A,
        //            "Périmètre confirmé pour validation du partage."));

        //    reponsePerimetreResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        //    // Act : Sarah demande la validation du nouveau partage.
        //    await AuthenticateAsync("sarah.dupont@example.com");

        //    var response = await _client.PostAsync(
        //        $"/api/partages/{partageId}/demande-validation",
        //        null);

        //    // Assert
        //    response.StatusCode.Should().Be(HttpStatusCode.OK);

        //    var dto = await response.Content.ReadFromJsonAsync<DemandeValidationPartageDto>();

        //    dto.Should().NotBeNull();
        //    dto!.PartageId.Should().Be(partageId);
        //    dto.DemandeId.Should().NotBeEmpty();
        //    dto.ResponseStatus.Should().Be(DdeGRDResponseStatus.EnAttente.ToString());
        //    dto.DetailsDemande.Should().Contain("Demande de validation d'un nouveau partage");
        //    dto.DetailsDemande.Should().Contain("Points d'accès participants");
        //}

        //[Fact]
        //public async Task DemandeValidationPartage_WhenPairToPairWithoutPerimetre_ShouldReturnBadRequest()
        //{
        //    // Arrange : Sarah crée un partage pair-à-pair complet, mais sans périmètre confirmé.
        //    await AuthenticateAsync("sarah.dupont@example.com");

        //    var createResponse = await _client.PostAsJsonAsync(
        //        "/api/partages",
        //        new CreatePartage("Partage sans périmètre", PartageEnergieType.PairToPair));

        //    createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        //    var partageId = await createResponse.Content.ReadFromJsonAsync<Guid>();

        //    var invitationResponse = await _client.PostAsync(
        //        $"/api/partages/{partageId}/invitation-code",
        //        null);

        //    invitationResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        //    var invitationDto = await invitationResponse.Content.ReadFromJsonAsync<InvitationCodeDto>();
        //    invitationDto.Should().NotBeNull();

        //    await AuthenticateAsync("hugo.lambert@example.com");

        //    var joinResponse = await _client.PostAsJsonAsync(
        //        "/api/partages/rejoindre",
        //        new RejoindrePartageRequest(invitationDto!.InvitationCode));

        //    joinResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        //    // Act : Sarah tente de demander la validation sans périmètre confirmé.
        //    await AuthenticateAsync("sarah.dupont@example.com");

        //    var response = await _client.PostAsync(
        //        $"/api/partages/{partageId}/demande-validation",
        //        null);

        //    // Assert
        //    response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        //}

        //[Fact]
        //public async Task DemandeValidationPartage_WhenUserIsNotSeller_ShouldReturnForbidden()
        //{
        //    // Arrange : Sarah crée un partage.
        //    await AuthenticateAsync("sarah.dupont@example.com");

        //    var createResponse = await _client.PostAsJsonAsync(
        //        "/api/partages",
        //        new CreatePartage("Partage validation interdit", PartageEnergieType.MemeBatiment));

        //    createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        //    var partageId = await createResponse.Content.ReadFromJsonAsync<Guid>();

        //    // Act : Julien tente de demander la validation du partage de Sarah.
        //    await AuthenticateAsync("julien.martin@example.com");

        //    var response = await _client.PostAsync(
        //        $"/api/partages/{partageId}/demande-validation",
        //        null);

        //    // Assert
        //    response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        //}

        //[Fact]
        //public async Task DemandeValidationPartage_WhenValidationAlreadyPending_ShouldReturnBadRequest()
        //{
        //    // Arrange : Sarah crée un partage même bâtiment.
        //    // Pour MêmeBatiment, le périmètre A est défini automatiquement par le handler.
        //    await AuthenticateAsync("sarah.dupont@example.com");

        //    var createResponse = await _client.PostAsJsonAsync(
        //        "/api/partages",
        //        new CreatePartage("Partage validation doublon", PartageEnergieType.MemeBatiment));

        //    createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        //    var partageId = await createResponse.Content.ReadFromJsonAsync<Guid>();

        //    var invitationResponse = await _client.PostAsync(
        //        $"/api/partages/{partageId}/invitation-code",
        //        null);

        //    invitationResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        //    var invitationDto = await invitationResponse.Content.ReadFromJsonAsync<InvitationCodeDto>();
        //    invitationDto.Should().NotBeNull();

        //    await AuthenticateAsync("lea.bernard@example.com");

        //    var joinResponse = await _client.PostAsJsonAsync(
        //        "/api/partages/rejoindre",
        //        new RejoindrePartageRequest(invitationDto!.InvitationCode));

        //    joinResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        //    await AuthenticateAsync("sarah.dupont@example.com");

        //    var firstResponse = await _client.PostAsync(
        //        $"/api/partages/{partageId}/demande-validation",
        //        null);

        //    firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        //    // Act : Sarah refait une deuxième demande alors qu'une demande est déjà en attente.
        //    var secondResponse = await _client.PostAsync(
        //        $"/api/partages/{partageId}/demande-validation",
        //        null);

        //    // Assert
        //    secondResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        //}







        ////authentification

        //private async Task AuthenticateAsync(string email, string password = "Test1234!")
        //{   
        //// erreur 429 --> trop de tenative de connexion -> solution passer en cache:
        //var token = email switch
        //    {
        //        "sarah.dupont@example.com" =>
        //            _sarahToken ??= await GetTokenAsync(email, password),

        //        "julien.martin@example.com" =>
        //            _julienToken ??= await GetTokenAsync(email, password),

        //        "admin.test@example.com" =>
        //            _adminToken ??= await GetTokenAsync(email, password),

        //        "lea.bernard@example.com" =>
        //            _leaToken ??= await GetTokenAsync(email, password),

        //        "agent.sibelga@example.com" =>
        //            _sibelgaToken ??= await GetTokenAsync(email, password),
        //        "hugo.lambert@example.com" =>
        //            _hugoToken ??= await GetTokenAsync(email, password),
        //        "contact@boulangerie-dupain.be" =>
        //            _boulangerieToken ??= await GetTokenAsync(email, password),

        //        _ => await GetTokenAsync(email, password)
        //    };


        //    //var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new
        //    //{
        //    //    email,
        //    //    password
        //    //});

        //    //loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        //    //var auth = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        //    //auth.Should().NotBeNull();
        //    //auth!.AccessToken.Should().NotBeNullOrWhiteSpace();

        //    _client.DefaultRequestHeaders.Authorization =
        //        new AuthenticationHeaderValue("Bearer", token);
        //}


        //private async Task<string> GetTokenAsync(string email, string password)
        //{
        //    var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new
        //    {
        //        email,
        //        password
        //    });

        //    var body = await loginResponse.Content.ReadAsStringAsync();

        //    loginResponse.StatusCode.Should().Be(
        //        HttpStatusCode.OK,
        //        $"Login échoué pour {email}. Réponse API : {body}");

        //    var auth = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();

        //    auth.Should().NotBeNull();
        //    auth!.AccessToken.Should().NotBeNullOrWhiteSpace();

        //    return auth.AccessToken;
        //}


        //private sealed class AuthResponse
        //{
        //    public string AccessToken { get; set; } = string.Empty;
        //    public string RefreshToken { get; set; } = string.Empty;
        //    public DateTime AccessTokenExpiresAt { get; set; }
        //}



        //private async Task<string> CreateInvitationCodeAsync()
        //{
        //    await AuthenticateAsync("sarah.dupont@example.com");

        //    var createResponse = await _client.PostAsJsonAsync("/api/partages",
        //        new CreatePartage("Test", PartageEnergieType.PairToPair));

        //    var partageId = await createResponse.Content.ReadFromJsonAsync<Guid>();

        //    var invitationResponse = await _client.PostAsync(
        //        $"/api/partages/{partageId}/invitation-code", null);

        //    var dto = await invitationResponse.Content.ReadFromJsonAsync<InvitationCodeDto>();

        //    return dto!.InvitationCode;
        //}
    }
}