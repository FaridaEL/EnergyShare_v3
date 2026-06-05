using FluentAssertions;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using EnergyShare_v3.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EnergyShare_v3.IntegrationTests
{
    public class PointAccessEndpointTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory _factory;

        public PointAccessEndpointTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetMyPointAccesses_WithoutAuthentication_ShouldReturnUnauthorized()
        {
            var response = await _client.GetAsync("/api/points-access/me");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetMyPointAccesses_WithSarahToken_ShouldReturnOk()
        {
            await AuthenticateAsSarahAsync();

            var response = await _client.GetAsync("/api/points-access/me");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task DeactivatePointAccess_WithoutAuthentication_ShouldReturnUnauthorized()
        {
            var pointAccessId = "1b73955c-7d4e-401d-969b-dce4faf2c735";

            var response = await _client.PostAsync($"/api/points-access/deactivate/{pointAccessId}", null);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task DeactivatePointAccess_WhenPointBelongsToAnotherUser_ShouldReturnForbidden()
        {
            await AuthenticateAsSarahAsync();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var otherUserPointAccessId = await db.PointAccesses
                .Where(pa => pa.User.Email != "sarah.dupont@example.com")
                .Select(pa => pa.Id)
                .FirstAsync();

            var response = await _client.PostAsync(
                $"/api/points-access/deactivate/{otherUserPointAccessId}",
                null);

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        private async Task AuthenticateAsSarahAsync()
        {
            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new
            {
                email = "sarah.dupont@example.com",
                password = "Test1234!"
            });

            loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var auth = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
            auth.Should().NotBeNull();
            auth!.AccessToken.Should().NotBeNullOrWhiteSpace();

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", auth.AccessToken);

            var debugResponse = await _client.GetAsync("/api/debug/me");
            var debugBody = await debugResponse.Content.ReadAsStringAsync();

            debugResponse.StatusCode.Should().Be(HttpStatusCode.OK, debugBody);
        }

        private sealed class AuthResponse
        {
            public string AccessToken { get; set; } = string.Empty;
            public string RefreshToken { get; set; } = string.Empty;
            public DateTime AccessTokenExpiresAt { get; set; }
        }
    }
}
