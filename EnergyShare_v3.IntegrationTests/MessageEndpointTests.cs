
using FluentAssertions;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using EnergyShare_v3.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EnergyShare_v3.IntegrationTests
{
    public class MessageEndpointTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory _factory;
        private static string? _sarahToken;
        private static string? _julienToken;

        public MessageEndpointTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetInbox_WithoutAuthentication_ShouldReturnUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/messages/inbox");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task SendMessage_WithoutAuthentication_ShouldReturnUnauthorized()
        {
            // Arrange
            var command = new
            {
                destinataireId = Guid.NewGuid(),
                objetMessage = "Test",
                contenu = "Bonjour",
                matchId = (Guid?)null
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/messages", command);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task SendMessage_WithSarahToken_ShouldReturnOk()
        {
            // Arrange
            await AuthenticateAsSarahAsync();

            var julienId = await GetUserIdByEmailAsync("julien.martin@example.com");

            var command = new
            {
                destinataireId = julienId,
                objetMessage = "Test intégration",
                contenu = "Bonjour Julien, ceci est un test.",
                matchId = (Guid?)null
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/messages", command);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetOutbox_AfterSendingMessage_ShouldContainMessage()
        {
            // Arrange
            await AuthenticateAsSarahAsync();

            var julienId = await GetUserIdByEmailAsync("julien.martin@example.com");

            var command = new
            {
                destinataireId = julienId,
                objetMessage = "Message outbox",
                contenu = "Ce message doit apparaître dans les messages envoyés.",
                matchId = (Guid?)null
            };

            await _client.PostAsJsonAsync("/api/messages", command);

            // Act
            var response = await _client.GetAsync("/api/messages/outbox");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var body = await response.Content.ReadAsStringAsync();
            body.Should().Contain("Message outbox");
        }

        [Fact]
        public async Task GetInbox_AsJulien_AfterSarahSentMessage_ShouldContainMessage()
        {
            // Arrange : Sarah envoie un message à Julien
            await AuthenticateAsSarahAsync();

            var julienId = await GetUserIdByEmailAsync("julien.martin@example.com");

            var command = new
            {
                destinataireId = julienId,
                objetMessage = "Message inbox",
                contenu = "Ce message doit apparaître dans l'inbox de Julien.",
                matchId = (Guid?)null
            };

            await _client.PostAsJsonAsync("/api/messages", command);

            // Arrange : on remplace le token Sarah par le token Julien
            await AuthenticateAsJulienAsync();

            // Act
            var response = await _client.GetAsync("/api/messages/inbox");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var body = await response.Content.ReadAsStringAsync();
            body.Should().Contain("Message inbox");
        }

        [Fact]
        public async Task MarkMessageAsRead_AsDestinataire_ShouldReturnOk()
        {
            // Arrange : Sarah envoie un message à Julien
            await AuthenticateAsSarahAsync();

            var julienId = await GetUserIdByEmailAsync("julien.martin@example.com");

            var command = new
            {
                destinataireId = julienId,
                objetMessage = "Message à lire",
                contenu = "Julien doit pouvoir marquer ce message comme lu.",
                matchId = (Guid?)null
            };

            await _client.PostAsJsonAsync("/api/messages", command);

            var messageId = await GetLastMessageIdForDestinataireAsync(julienId);

            // Arrange : Julien se connecte
            await AuthenticateAsJulienAsync();

            // Act
            var response = await _client.PutAsync($"/api/messages/{messageId}/read", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task MarkMessageAsRead_WhenMessageBelongsToAnotherUser_ShouldReturnForbidden()
        {
            // Arrange : Sarah envoie un message à Julien
            await AuthenticateAsSarahAsync();

            var julienId = await GetUserIdByEmailAsync("julien.martin@example.com");

            var command = new
            {
                destinataireId = julienId,
                objetMessage = "Message interdit",
                contenu = "Sarah ne doit pas pouvoir marquer ce message reçu par Julien comme lu.",
                matchId = (Guid?)null
            };

            await _client.PostAsJsonAsync("/api/messages", command);

            var messageId = await GetLastMessageIdForDestinataireAsync(julienId);

            // Act : Sarah tente de marquer comme lu un message dont elle n'est pas destinataire
            var response = await _client.PutAsync($"/api/messages/{messageId}/read", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        private async Task AuthenticateAsSarahAsync()
        {
            //await AuthenticateAsync("sarah.dupont@example.com", "Test1234");

            //les tests échouent car Sarh doit se relogger pour chaque test donc on la met en cache pour éviter l'erreur 429 TooManyRequests 
            _sarahToken ??= await GetTokenAsync("sarah.dupont@example.com", "Test1234");

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _sarahToken);
        }

        private async Task AuthenticateAsJulienAsync()
        {
            //await AuthenticateAsync("julien.martin@example.com", "Test1234");

            //le ss tests échouent car Julien doit se relogger pour chaque test donc on le met en cache pour éviter les appels redondants à l'endpoint de login (et ainsi éviter les problèmes de tokens invalides)
            _julienToken ??= await GetTokenAsync("julien.martin@example.com", "Test1234");

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _julienToken);
        }


        //private async Task AuthenticateAsync(string email, string password)
        //{
        //    _client.DefaultRequestHeaders.Authorization = null;

        //    var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new
        //    {
        //        email,
        //        password
        //    });

        //    loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        //    var auth = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();

        //    auth.Should().NotBeNull();
        //    auth!.AccessToken.Should().NotBeNullOrWhiteSpace();

        //    _client.DefaultRequestHeaders.Authorization =
        //        new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        //}
        private async Task<string> GetTokenAsync(string email, string password)
        {
            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new
            {
                email,
                password
            });

            var body = await loginResponse.Content.ReadAsStringAsync();

            loginResponse.StatusCode.Should().Be(
                HttpStatusCode.OK,
                $"Login échoué pour {email}. Réponse API : {body}");

            var auth = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();

            auth.Should().NotBeNull();
            auth!.AccessToken.Should().NotBeNullOrWhiteSpace();

            return auth.AccessToken;
        }

        private async Task<Guid> GetUserIdByEmailAsync(string email)
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            return await db.Users
                .Where(u => u.Email == email)
                .Select(u => u.Id)
                .FirstAsync();
        }

        private async Task<Guid> GetLastMessageIdForDestinataireAsync(Guid destinataireId)
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            return await db.Messages
                .Where(m => m.DestinataireId == destinataireId)
                .OrderByDescending(m => m.DateEnvoi)
                .Select(m => m.Id)
                .FirstAsync();
        }

        private sealed class AuthResponse
        {
            public string AccessToken { get; set; } = string.Empty;
            public string RefreshToken { get; set; } = string.Empty;
            public DateTime AccessTokenExpiresAt { get; set; }
        }
    }
}