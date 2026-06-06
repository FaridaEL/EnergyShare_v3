using EnergyShare_v3.Application.Features.Partage;
using EnergyShare_v3.Domain.Enums;
using EnergyShare_v3.IntegrationTests.Common;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace EnergyShare_v3.IntegrationTests.Partage
{
    public class PartageCreateEndpointTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly TestDataFactory _dataFactory;
        public PartageCreateEndpointTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
            _dataFactory = new TestDataFactory(_client);
        }

        [Fact]
        public async Task CreatePartage_WithoutAuthentication_ShouldReturnUnauthorized()
        {
            var command = new CreatePartage(
                Nom: "Partage Test Integration",
                EnergieType: PartageEnergieType.PairToPair);

            var response = await _client.PostAsJsonAsync("/api/partages", command);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task CreatePartage_WithSellerToken_ShouldReturnCreated()
        {
            var sellerEmail = await _dataFactory.CreateSellerWithInjectionPointAsync();

            await TestAuthHelper.AuthenticateAsync(_client, sellerEmail);
            //await AuthenticateAsync("sarah.dupont@example.com");

            var command = new CreatePartage(
                Nom: "Partage Test Integration",
                EnergieType: PartageEnergieType.PairToPair);

            var response = await _client.PostAsJsonAsync("/api/partages", command);

            var body = await response.Content.ReadAsStringAsync();
            response.StatusCode.Should().Be(HttpStatusCode.Created,
                $"Réponse API : {body}");

            var partageId = await response.Content.ReadFromJsonAsync<Guid>();
            partageId.Should().NotBeEmpty();
        }

       

    }
}