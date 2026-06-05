using EnergyShare_v3.Infrastructure.Database;
using EnergyShare_v3.IntegrationTests.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

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

        /// <summary>
        /// Vérifie qu'un utilisateur non authentifié ne peut pas consulter ses points d'accès.
        /// </summary>
        [Fact]
        public async Task GetMyPointAccesses_WithoutAuthentication_ShouldReturnUnauthorized()
        {
            var response = await _client.GetAsync("/api/points-access/me");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        /// <summary>
        /// Vérifie qu'un utilisateur authentifié peut consulter ses propres points d'accès.
        /// </summary>
        [Fact]
        public async Task GetMyPointAccesses_WithSarahToken_ShouldReturnOk()
        {
            await TestAuthHelper.AuthenticateAsync(
            _client,
            TestUsers.Sarah);

            var response = await _client.GetAsync("/api/points-access/me");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        /// <summary>
        /// Vérifie qu'un utilisateur non authentifié ne peut pas désactiver un point d'accès.
        /// </summary>
        [Fact]
        public async Task DeactivatePointAccess_WithoutAuthentication_ShouldReturnUnauthorized()
        {
            var pointAccessId = Guid.NewGuid(); 

            var response = await _client.PostAsync($"/api/points-access/deactivate/{pointAccessId}", null);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        /// <summary>
        /// Vérifie qu'un utilisateur ne peut pas désactiver
        /// le point d'accès appartenant à un autre utilisateur.
        /// </summary>
        [Fact]
        public async Task DeactivatePointAccess_WhenPointBelongsToAnotherUser_ShouldReturnForbidden()
        {
            await TestAuthHelper.AuthenticateAsync(
            _client,
            TestUsers.Sarah);

            var otherUserPointAccessId =
            await GetPointAccessIdNotOwnedByAsync(TestUsers.Sarah);

            var response = await _client.PostAsync(
                $"/api/points-access/deactivate/{otherUserPointAccessId}",
                null);

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        /// <summary>
        /// Récupère un point d'accès appartenant à un autre utilisateur
        /// que celui fourni en paramètre.
        /// </summary>
        private async Task<Guid> GetPointAccessIdNotOwnedByAsync(string userEmail)
        {
            using var scope = _factory.Services.CreateScope();

            var db = scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

            return await db.PointAccesses
                .Where(pa => pa.User.Email != userEmail)
                .Select(pa => pa.Id)
                .FirstAsync();
        }
    }
}
