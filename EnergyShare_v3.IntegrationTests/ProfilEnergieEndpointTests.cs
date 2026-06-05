using EnergyShare_v3.Infrastructure.Database;
using EnergyShare_v3.IntegrationTests.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

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
        /// <summary>
        /// Vérifie qu'un utilisateur non authentifié ne peut pas consulter ses profils énergie.
        /// </summary>
        [Fact]
        public async Task GetMyProfilEnergie_WithoutAuthentication_ShouldReturnUnauthorized()
        {
            var response = await _client.GetAsync("/api/profils-energie/me");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        /// <summary>
        /// Vérifie qu'un utilisateur authentifié peut consulter ses profils énergie.
        /// </summary>
        [Fact]
        public async Task GetMyProfilEnergie_WithSarahToken_ShouldReturnOk()
        {
            await TestAuthHelper.AuthenticateAsync(
            _client,
            TestUsers.Sarah);

            var response = await _client.GetAsync("/api/profils-energie/me");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        /// <summary>
        /// Vérifie qu'un utilisateur non authentifié ne peut pas modifier un profil énergie.
        /// </summary>
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

        /// <summary>
        /// Vérifie qu'un utilisateur peut modifier son propre profil énergie.
        /// </summary>
        [Fact]
        public async Task UpdateProfilEnergie_WithSarahToken_ShouldReturnOk()
        {
            await TestAuthHelper.AuthenticateAsync(
           _client,
           TestUsers.Sarah);

           var profilId = await GetProfilIdByUserEmailAsync(TestUsers.Sarah);

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

        /// <summary>
        /// Vérifie qu'un utilisateur ne peut pas consulter le profil énergie détaillé d'un autre utilisateur.
        /// </summary>
        
        [Fact]    
        public async Task GetProfilEnergieById_WhenProfilBelongsToAnotherUser_ShouldReturnForbiddenOrNotFound()
        {
            await TestAuthHelper.AuthenticateAsync(
            _client,
            TestUsers.Sarah);

            var otherProfilId = await GetProfilIdNotOwnedByAsync(TestUsers.Sarah);

            // 3. Sarah tente d'accéder à CE profil (qui ne lui appartient pas)
            var response = await _client.GetAsync($"/api/profils-energie/{otherProfilId}");

            // 4. Comportement attendu :
            // l'API refuse l'accès direct au profil détaillé d'un autre utilisateur.
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        /// <summary>
        /// Récupère le profil énergie d'un utilisateur.
        /// </summary>
        private async Task<Guid> GetProfilIdByUserEmailAsync(
            string userEmail)
        {
            using var scope = _factory.Services.CreateScope();

            var db = scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

            return await db.ProfilsEnergie
                .Where(p => p.PointAccess.User.Email == userEmail)
                .Select(p => p.Id)
                .FirstAsync();
        }

        /// <summary>
        /// Récupère un profil énergie appartenant à un autre utilisateur.
        /// </summary>
        private async Task<Guid> GetProfilIdNotOwnedByAsync(
            string userEmail)
        {
            using var scope = _factory.Services.CreateScope();

            var db = scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

            return await db.ProfilsEnergie
                .Where(p => p.PointAccess.User.Email != userEmail)
                .Select(p => p.Id)
                .FirstAsync();
        }
    }
}