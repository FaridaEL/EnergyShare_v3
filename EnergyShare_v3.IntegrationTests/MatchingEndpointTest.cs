using EnergyShare_v3.Infrastructure.Database;
using EnergyShare_v3.IntegrationTests.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

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
			await TestAuthHelper.AuthenticateAsync(_client, TestUsers.Sarah);

			var sourcePointAccessId = await GetPointAccessIdAsync(TestUsers.Sarah);

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
			await TestAuthHelper.AuthenticateAsync(_client, TestUsers.Sarah);

			var response = await _client.GetAsync("/api/matching");

			response.StatusCode.Should().Be(HttpStatusCode.OK);
		}


		private async Task<Guid> GetPointAccessIdAsync(string userEmail)
		{
			using var scope = _factory.Services.CreateScope();

			var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

			return await db.PointAccesses
				.Where(pa => pa.User.Email == userEmail)
				.Select(pa => pa.Id)
				.FirstAsync();
		}

    }
}