using FluentAssertions;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using EnergyShare_v3.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EnergyShare_v3.IntegrationTests
{
    public class MatchingEndpointTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory _factory;

        public MatchingEndpointTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task SearchPotentialMatches_WithoutAuthentication_ShouldReturnUnauthorized()
        {
            var pointAccessId = Guid.NewGuid();

            var response = await _client.GetAsync($"/api/matching/potential/{pointAccessId}");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task SearchPotentialMatches_WithSarahToken_ShouldReturnOk()
        {
            await AuthenticateAsSarahAsync();

            var sourcePointAccessId = await GetSarahPointAccessIdAsync();

            var response = await _client.GetAsync($"/api/matching/potential/{sourcePointAccessId}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetMatches_WithoutAuthentication_ShouldReturnUnauthorized()
        {
            var response = await _client.GetAsync("/api/matching");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetMatches_WithSarahToken_ShouldReturnOk()
        {
            await AuthenticateAsSarahAsync();

            var response = await _client.GetAsync("/api/matching");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        private async Task<Guid> GetSarahPointAccessIdAsync()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            return await db.PointAccesses
                .Where(pa => pa.User.Email == "sarah.dupont@example.com")
                .Select(pa => pa.Id)
                .FirstAsync();
        }

        private async Task AuthenticateAsSarahAsync()
        {
            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new
            {
                email = "sarah.dupont@example.com",
                password = "Test1234"
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