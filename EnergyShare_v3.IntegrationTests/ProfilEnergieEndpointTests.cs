using FluentAssertions;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using EnergyShare_v3.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EnergyShare_v3.IntegrationTests
{
    public class ProfilEnergieEndpointTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory _factory;

        public ProfilEnergieEndpointTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetMyProfilEnergie_WithoutAuthentication_ShouldReturnUnauthorized()
        {
            var response = await _client.GetAsync("/api/profils-energie/me");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetMyProfilEnergie_WithSarahToken_ShouldReturnOk()
        {
            await AuthenticateAsSarahAsync();

            var response = await _client.GetAsync("/api/profils-energie/me");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task UpdateProfilEnergie_WithoutAuthentication_ShouldReturnUnauthorized()
        {
            var profilId = Guid.NewGuid();

            var request = new
            {
                demandeEnergie_kWh = 1200m,
                offreEnergie_kWh = 0m,
                prixAchatCible_Eur = 0.18m,
                prixVenteCible_Eur = (decimal?)null
            };

            var response = await _client.PutAsJsonAsync(
                $"/api/profils-energie/{profilId}",
                request);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task UpdateProfilEnergie_WithSarahToken_ShouldReturnOk()
        {
            await AuthenticateAsSarahAsync();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var profilId = await db.ProfilsEnergie
                .Where(p => p.PointAccess.User.Email == "sarah.dupont@example.com")
                .Select(p => p.Id)
                .FirstAsync();

            var request = new
            {
                demandeEnergie_kWh = 1500m,
                offreEnergie_kWh = 0m,
                prixAchatCible_Eur = 0.17m,
                prixVenteCible_Eur = (decimal?)null
            };

            var response = await _client.PutAsJsonAsync(
                $"/api/profils-energie/{profilId}",
                request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]     //Todo : sécuriser handler GetById en ajoutant context-user..
        public async Task GetProfilEnergieById_WhenProfilBelongsToAnotherUser_ShouldReturnForbiddenOrNotFound()
        {
            // 1. On simule une connexion en tant que Sarah
            await AuthenticateAsSarahAsync();

            // 2. On récupère un profil énergie qui n'appartient PAS à Sarah
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var otherProfilId = await db.ProfilsEnergie
                .Where(p => p.PointAccess.User.Email != "sarah.dupont@example.com")
                .Select(p => p.Id)
                .FirstAsync();

            // 3. Sarah tente d'accéder à CE profil (qui ne lui appartient pas)
            var response = await _client.GetAsync($"/api/profils-energie/{otherProfilId}");

            // 4. Comportement attendu :
            // l'API refuse l'accès direct au profil détaillé d'un autre utilisateur.
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
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