using EnergyShare_v3.IntegrationTests.Common;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace EnergyShare_v3.IntegrationTests
{
    public class UsersEndpointTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public UsersEndpointTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetUsers_WithoutAuthentication_ShouldReturnUnauthorized()
        {
            // /api/users est réservé aux admins : un visiteur anonyme doit être refusé.
            var response = await _client.GetAsync("/api/users");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetMyProfile_WithoutAuthentication_ShouldReturnUnauthorized()
        {
            // /api/users/me nécessite un JWT valide.
            var response = await _client.GetAsync("/api/users/me");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetMyProfile_WithUserToken_ShouldReturnCurrentUser()
        {
            await TestAuthHelper.AuthenticateAsync(_client, TestUsers.Sarah);

            var response = await _client.GetAsync("/api/users/me");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var profile = await response.Content.ReadFromJsonAsync<CurrentUserProfileResponse>();

            profile.Should().NotBeNull();
            profile!.Email.Should().Be("sarah.dupont@example.com");
            profile.FirstName.Should().Be("Sarah");
            profile.LastName.Should().Be("Dupont");
            profile.Role.Should().Be("Utilisateur");
        }

        [Fact]
        public async Task UpdateMyProfile_WithUserToken_ShouldUpdatePhoneNumber()
        {
            await TestAuthHelper.AuthenticateAsync(_client, TestUsers.Sarah);

            var updateResponse = await _client.PutAsJsonAsync("/api/users/me", new
            {
                firstName = "Sarah",
                lastName = "Dupont",
                phoneNumber = "0470000099",
                societeName = (string?)null,
                numeroEntreprise = (string?)null
            });

            updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var getResponse = await _client.GetAsync("/api/users/me");
            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var profile = await getResponse.Content.ReadFromJsonAsync<CurrentUserProfileResponse>();

            profile.Should().NotBeNull();
            profile!.PhoneNumber.Should().Be("0470000099");
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

        [Fact]
        public async Task Home_ShouldReturnSuccess()
        {
            var response = await _client.GetAsync("/");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        private sealed class CurrentUserProfileResponse
        {
            public Guid Id { get; set; }
            public string Email { get; set; } = string.Empty;
            public string? FirstName { get; set; }
            public string? LastName { get; set; }
            public string? PhoneNumber { get; set; }
            public string? SocieteName { get; set; }
            public string? NumeroEntreprise { get; set; }
            public string Status { get; set; } = string.Empty;
            public string Role { get; set; } = string.Empty;
        }
    }
}