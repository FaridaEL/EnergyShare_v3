using FluentAssertions;
using System.Net;
using System.Net.Http.Headers;
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
            var token = await LoginAndGetAccessTokenAsync(
                "sarah.dupont@example.com",
                "Test1234");

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

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
            var token = await LoginAndGetAccessTokenAsync(
                "sarah.dupont@example.com",
                "Test1234");

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

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

        private async Task<string> LoginAndGetAccessTokenAsync(string email, string password)
        {
            // Les tests utilisent le seed de test : Sarah existe avec le rôle Utilisateur.
            var response = await _client.PostAsJsonAsync("/api/auth/login", new
            {
                email,
                password
            });

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var auth = await response.Content.ReadFromJsonAsync<AuthTestResponse>();

            auth.Should().NotBeNull();
            auth!.AccessToken.Should().NotBeNullOrWhiteSpace();

            return auth.AccessToken;
        }

        private sealed class AuthTestResponse
        {
            public string AccessToken { get; set; } = string.Empty;
            public string RefreshToken { get; set; } = string.Empty;
            public DateTime AccessTokenExpiresAt { get; set; }
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