using EnergyShare_v3.IntegrationTests.Common;
using FluentAssertions;
using System.Net;

namespace EnergyShare_v3.IntegrationTests.Partage;

public class PartageHistoriqueEndpointTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public PartageHistoriqueEndpointTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    /// <summary>
    /// Vérifie qu'un utilisateur non authentifié ne peut pas consulter l'historique d'un partage.
    /// </summary>
    [Fact]
    public async Task GetHistoriqueDemandesGrd_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Act
        var response = await _client.GetAsync( $"/api/partages/{Guid.NewGuid()}/historique-demandes-grd");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}