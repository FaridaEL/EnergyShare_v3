using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Text;

namespace EnergyShare_v3.IntegrationTests
{
    public class UsersEndpointTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public UsersEndpointTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]//(Skip = "À reprendre après stabilisation complète de l'authentification API et du WebApplicationFactory.")]
        /*public async Task GetUsers_ShouldReturnOk_AndAListOfUsers()  //un user anonyme peut voir tout les users
        {
            // Act
            var response = await _client.GetAsync("/api/users");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var users = await response.Content.ReadFromJsonAsync<List<UserResponse>>();

            users.Should().NotBeNull();
            users.Should().NotBeEmpty();
            users!.Count.Should().BeGreaterThan(0);
        } */
        public async Task GetUsers_WithoutAuthentication_ShouldReturnUnauthorized()  //Seuls les admins peuvent voir la liste des users, un user anonyme ne peut pas y accéder
        {
            var response = await _client.GetAsync("/api/users");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]//(Skip = "À reprendre après stabilisation complète de l'authentification API et du WebApplicationFactory.")]  //mini test de diagnostic : vérifier que l'endpoint existe et ne retourne pas 404.
        public async Task DebugMe_ShouldExist()
        {
            var response = await _client.GetAsync("/api/debug/me");

            response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Home_ShouldReturnSuccess()
        {
            var response = await _client.GetAsync("/");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task LoginEndpoint_ShouldExist()
        {
            var response = await _client.PostAsJsonAsync("/api/auth/login", new
            {
                email = "fake@test.com",
                password = "wrongpassword"
            });

            response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        }


        private sealed class UserResponse
        {
            public Guid Id { get; set; }
            public string? FirstName { get; set; }
            public string? LastName { get; set; }
            public string Email { get; set; } = string.Empty;
            public string Role { get; set; } = string.Empty;
            public DateTime CreatedAt { get; set; }
        }
    }
}
