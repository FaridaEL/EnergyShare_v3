using EnergyShare_v3.Application.Features.Partage;
using EnergyShare_v3.Domain.Enums;
using EnergyShare_v3.IntegrationTests.Common;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace EnergyShare_v3.IntegrationTests.Partage;

public class PartageModificationEndpointTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly TestDataFactory _dataFactory;

    public PartageModificationEndpointTests(
        CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _dataFactory = new TestDataFactory(_client);
    }

    /// <summary>
    /// Vérifie qu'un utilisateur non authentifié ne peut pas demander la modification d'un partage.
    /// </summary>
    [Fact]
    public async Task DemandeModification_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Act
        var response = await _client.PostAsync(
            $"/api/partages/{Guid.NewGuid()}/demande-modification",
            null);

        // Assert
        response.StatusCode.Should()
            .Be(HttpStatusCode.Unauthorized);
    }
  
    /// Vérifie qu'un utilisateur connecté qui n'est pas le vendeur du partage ne peut pas demander sa modification.
   
    [Fact]
    public async Task DemandeModification_WhenUserIsNotSeller_ShouldReturnForbidden()
    {
        // Arrange
        // Création d'un vrai vendeur avec un point d'injection actif.
        // Après cet appel, le client est authentifié avec ce vendeur.
        var seller = await _dataFactory
            .CreateSellerWithInjectionPointDataAsync();

        // Création d'un vrai partage appartenant au vendeur.
        var createResponse = await _client.PostAsJsonAsync(
            "/api/partages",
            new CreatePartage(
                "Partage modification sécurisé",
                PartageEnergieType.PairToPair,
                seller.PointAccessId));

        createResponse.StatusCode.Should()
            .Be(HttpStatusCode.Created);

        var partageId = await createResponse.Content
            .ReadFromJsonAsync<Guid>();

        partageId.Should().NotBeEmpty();

        // On change ensuite d'utilisateur --> Julien qui est dans les utilisateurs seedés de test..
        await TestAuthHelper.AuthenticateAsync(
            _client,
            TestUsers.Julien);

        // Act
        var response = await _client.PostAsync(
            $"/api/partages/{partageId}/demande-modification",
            null);

        // Assert
        response.StatusCode.Should()
            .Be(HttpStatusCode.Forbidden);
    }
}