
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
    public class MessageEndpointTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory _factory;

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
            await TestAuthHelper.AuthenticateAsync(_client, TestUsers.Sarah);

            var julienId = await GetUserIdByEmailAsync(TestUsers.Julien);

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
            await TestAuthHelper.AuthenticateAsync(_client, TestUsers.Sarah);

            var julienId = await GetUserIdByEmailAsync(TestUsers.Julien);

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
            await TestAuthHelper.AuthenticateAsync(_client, TestUsers.Sarah);

            var julienId = await GetUserIdByEmailAsync(TestUsers.Julien);

            var command = new
            {
                destinataireId = julienId,
                objetMessage = "Message inbox",
                contenu = "Ce message doit apparaître dans l'inbox de Julien.",
                matchId = (Guid?)null
            };

            await _client.PostAsJsonAsync("/api/messages", command);

            // Arrange : on remplace le token Sarah par le token Julien
            await TestAuthHelper.AuthenticateAsync(_client, TestUsers.Julien);

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
            await TestAuthHelper.AuthenticateAsync(_client, TestUsers.Sarah);

            var julienId = await GetUserIdByEmailAsync(TestUsers.Julien);

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
            await TestAuthHelper.AuthenticateAsync(_client, TestUsers.Julien);

            // Act
            var response = await _client.PutAsync($"/api/messages/{messageId}/read", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task MarkMessageAsRead_WhenMessageBelongsToAnotherUser_ShouldReturnForbidden()
        {
            // Arrange : Sarah envoie un message à Julien
            await TestAuthHelper.AuthenticateAsync(_client, TestUsers.Sarah);

            var julienId = await GetUserIdByEmailAsync(TestUsers.Julien);

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
    }
}