using EnergyShare_v3.Application.Features.Partage;
using EnergyShare_v3.Domain.Enums;
using EnergyShare_v3.Web.Models.Partage;
using FluentAssertions;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace EnergyShare_v3.IntegrationTests.Partage;

public class PartageValidationEndpointTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    private static string? _sarahToken;
    private static string? _hugoToken;
    private static string? _sibelgaToken;

    public PartageValidationEndpointTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    // =========================================================
    // TEST COMPLET : Description d'un scénario complet  Meme bâtiment de validation GRD.
    // - Sarah crée un partage
    // - Hugo rejoint le partage
    // - Sarah demande la validation GRD
    // - Le GRD valide le partage
    // - Le partage devient ACTIF
    // =========================================================

    [Fact]
    public async Task RepondreDemandeValidationPartage_WhenGrdValidates_ShouldReturnOk()
    {
        // =====================================================
        // 1. Sarah crée un partage
        // =====================================================

        await AuthenticateAsync("sarah.dupont@example.com");

        var createResponse = await _client.PostAsJsonAsync(
            "/api/partages",
            new CreatePartage(
                "Partage validation GRD",
                PartageEnergieType.MemeBatiment));

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var partageId = await createResponse.Content.ReadFromJsonAsync<Guid>();

        // =====================================================
        // 2. Sarah génère un code d'invitation
        // =====================================================

        var invitationResponse = await _client.PostAsync(
            $"/api/partages/{partageId}/invitation-code",
            null);

        invitationResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var invitationDto = await invitationResponse.Content
            .ReadFromJsonAsync<InvitationCodeDto>();

        invitationDto.Should().NotBeNull();

        // =====================================================
        // 3. Hugo rejoint le partage
        // =====================================================

        await AuthenticateAsync("hugo.lambert@example.com");

        var joinResponse = await _client.PostAsJsonAsync(
            "/api/partages/rejoindre",
            new RejoindrePartageRequest(invitationDto!.InvitationCode));

        joinResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // =====================================================
        // 4. Sarah demande la validation GRD
        // =====================================================

        await AuthenticateAsync("sarah.dupont@example.com");

        var demandeResponse = await _client.PostAsync(
            $"/api/partages/{partageId}/demande-validation",
            null);

        demandeResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var demandeDto = await demandeResponse.Content
            .ReadFromJsonAsync<DemandeValidationPartageDto>();

        demandeDto.Should().NotBeNull();

        // =====================================================
        // 5. Le GRD valide le partage
        // =====================================================

        await AuthenticateAsync("agent.sibelga@example.com");

        var response = await _client.PostAsJsonAsync(
            $"/api/partages/demandes-grd/{demandeDto!.DemandeId}/validation/repondre",
            new RepondreDemandeValidationPartageRequest(
                true,
                "Partage validé par le GRD."));

        // =====================================================
        // 6. Vérifications
        // =====================================================

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var dto = await response.Content
            .ReadFromJsonAsync<ReponseDemandeValidationPartageDto>();

        dto.Should().NotBeNull();

        dto!.ResponseStatus.Should().Be("Valide");

        dto.StatutPartage.Should()
            .Be(PartageEnergieStatutType.Actif);

        dto.CommentaireReponseGRD.Should()
            .Be("Partage validé par le GRD.");
    }

    // =========================================================
    // TEST SÉCURITÉ :
    // un utilisateur normal ne peut pas répondre
    // à une validation GRD.
    // =========================================================

    [Fact]
    public async Task RepondreDemandeValidationPartage_WhenUserIsNotGrd_ShouldReturnForbidden()
    {
        // Arrange
        await AuthenticateAsync("sarah.dupont@example.com");

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/partages/demandes-grd/{Guid.NewGuid()}/validation/repondre",
            new RepondreDemandeValidationPartageRequest(
                true,
                "Tentative interdite."));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // =========================================================
    // TEST MÉTIER :
    // le GRD refuse le partage.
    // Le partage doit redevenir INACTIF.
    // =========================================================

    [Fact]
    public async Task RepondreDemandeValidationPartage_WhenGrdRefuses_ShouldReturnOk()
    {
        // =====================================================
        // 1. Création partage
        // =====================================================

        await AuthenticateAsync("sarah.dupont@example.com");

        var createResponse = await _client.PostAsJsonAsync(
            "/api/partages",
            new CreatePartage(
                "Partage refusé",
                PartageEnergieType.MemeBatiment));// ne requiert pas de dde de périmetre traité préalable.

        var partageId = await createResponse.Content.ReadFromJsonAsync<Guid>();

        // =====================================================
        // 2. Invitation
        // =====================================================

        var invitationResponse = await _client.PostAsync(
            $"/api/partages/{partageId}/invitation-code",
            null);

        var invitationDto = await invitationResponse.Content
            .ReadFromJsonAsync<InvitationCodeDto>();

        // =====================================================
        // 3. Hugo rejoint
        // =====================================================

        await AuthenticateAsync("hugo.lambert@example.com");

        await _client.PostAsJsonAsync(
            "/api/partages/rejoindre",
            new RejoindrePartageRequest(invitationDto!.InvitationCode));

        // =====================================================
        // 4. Sarah demande validation GRD
        // =====================================================

        await AuthenticateAsync("sarah.dupont@example.com");

        var demandeResponse = await _client.PostAsync(
            $"/api/partages/{partageId}/demande-validation",
            null);

        var demandeDto = await demandeResponse.Content
            .ReadFromJsonAsync<DemandeValidationPartageDto>();

        // =====================================================
        // 5. GRD refuse le partage
        // =====================================================

        await AuthenticateAsync("agent.sibelga@example.com");

        var response = await _client.PostAsJsonAsync(
            $"/api/partages/demandes-grd/{demandeDto!.DemandeId}/validation/repondre",
            new RepondreDemandeValidationPartageRequest(
                false,
                "Le dossier est incomplet."));

        // =====================================================
        // 6. Vérifications
        // =====================================================

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var dto = await response.Content
            .ReadFromJsonAsync<ReponseDemandeValidationPartageDto>();

        dto.Should().NotBeNull();

        dto!.ResponseStatus.Should().Be("Refus");

        dto.StatutPartage.Should()
            .Be(PartageEnergieStatutType.Inactif);

        dto.CommentaireReponseGRD.Should()
            .Be("Le dossier est incomplet.");
    }

    // =========================================================
    // AUTH HELPERS
    // =========================================================

    private async Task AuthenticateAsync(
        string email,
        string password = "Test1234!")
    {
        var token = email switch
        {
            "sarah.dupont@example.com" =>
                _sarahToken ??= await GetTokenAsync(email, password),

            "hugo.lambert@example.com" =>
                _hugoToken ??= await GetTokenAsync(email, password),

            "agent.sibelga@example.com" =>
                _sibelgaToken ??= await GetTokenAsync(email, password),

            _ => await GetTokenAsync(email, password)
        };

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    private async Task<string> GetTokenAsync(
        string email,
        string password)
    {
        var loginResponse = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new
            {
                email,
                password
            });

        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var auth = await loginResponse.Content
            .ReadFromJsonAsync<AuthResponse>();

        auth.Should().NotBeNull();

        return auth!.AccessToken;
    }

    private sealed class AuthResponse
    {
        public string AccessToken { get; set; } = string.Empty;
    }
}