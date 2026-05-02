using FluentAssertions;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using EnergyShare_v3.Application.Features.Partage;
using EnergyShare_v3.Domain.Enums;

namespace EnergyShare_v3.IntegrationTests
{
    public class PartageEndpointTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public PartageEndpointTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
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
        public async Task CreatePartage_WithSarahToken_ShouldReturnCreated()
        {
            await AuthenticateAsync("sarah.dupont@example.com");

            var command = new CreatePartage(
                Nom: "Partage Test Integration",
                EnergieType: PartageEnergieType.PairToPair);

            var response = await _client.PostAsJsonAsync("/api/partages", command);

            response.StatusCode.Should().Be(HttpStatusCode.Created);

            var partageId = await response.Content.ReadFromJsonAsync<Guid>();
            partageId.Should().NotBeEmpty();
        }

        [Fact]
        public async Task GetPartageById_WhenUserIsSeller_ShouldReturnOk()
        {
            await AuthenticateAsync("sarah.dupont@example.com");

            var command = new CreatePartage(
                Nom: "Partage Seller Access Test",
                EnergieType: PartageEnergieType.PairToPair);

            var createResponse = await _client.PostAsJsonAsync("/api/partages", command);
            createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            var partageId = await createResponse.Content.ReadFromJsonAsync<Guid>();

            var response = await _client.GetAsync($"/api/partages/{partageId}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var dto = await response.Content.ReadFromJsonAsync<PartageDetailsDto>();
            dto.Should().NotBeNull();
            dto!.Id.Should().Be(partageId);
            dto.Nom.Should().Be("Partage Seller Access Test");
            dto.NombreParticipants.Should().Be(0);
        }

        [Fact]
        public async Task GetPartages_WithoutAuthentication_ShouldReturnUnauthorized()
        {
            var response = await _client.GetAsync("/api/partages");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetPartages_WhenUserIsNotAdmin_ShouldReturnForbidden()
        {
            await AuthenticateAsync("julien.martin@example.com"); 

            var response = await _client.GetAsync("/api/partages");

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetPartages_WhenUserIsAdmin_ShouldReturnOk()
        {
            await AuthenticateAsync("admin.test@example.com");

            var response = await _client.GetAsync("/api/partages");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var partages = await response.Content.ReadFromJsonAsync<List<PartageSummaryDto>>();
            partages.Should().NotBeNull();
        }

        private async Task AuthenticateAsync(string email, string password = "Test1234")
        {
            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new
            {
                email,
                password
            });

            loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var auth = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
            auth.Should().NotBeNull();
            auth!.AccessToken.Should().NotBeNullOrWhiteSpace();

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        }

        private sealed class AuthResponse
        {
            public string AccessToken { get; set; } = string.Empty;
            public string RefreshToken { get; set; } = string.Empty;
            public DateTime AccessTokenExpiresAt { get; set; }
        }
    }
}