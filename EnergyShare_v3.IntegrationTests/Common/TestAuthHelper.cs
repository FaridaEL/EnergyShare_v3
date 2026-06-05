using FluentAssertions;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace EnergyShare_v3.IntegrationTests.Common;

/// <summary>
/// Helper d'authentification pour les tests d'intégration.
///
/// Il effectue un vrai login via l'API puis place le JWT obtenu
/// dans le header Authorization du HttpClient.
///
/// Important :
/// on ne garde pas de cache statique de token.
/// Chaque CustomWebApplicationFactory utilise une base SQLite mémoire différente.
/// Un token généré dans une ancienne base peut contenir un UserId inexistant
/// dans la base actuelle, ce qui provoque des erreurs de clé étrangère.
/// </summary>
public static class TestAuthHelper
{
    /// <summary>
    /// Authentifie un utilisateur et configure le HttpClient avec le header :
    ///
    /// Authorization: Bearer {token}
    ///
    /// Après l'appel à cette méthode, toutes les requêtes envoyées avec ce HttpClient
    /// seront considérées comme authentifiées avec cet utilisateur.
    /// </summary>
    public static async Task AuthenticateAsync(
        HttpClient client,
        string email,
        string password = TestUsers.DefaultPassword)
    {
        var token = await GetTokenAsync(client, email, password);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    /// <summary>
    /// Récupère le token JWT d'un utilisateur.
    ///
    /// Si un token existe déjà dans le cache pour cet email, il est réutilisé.
    /// Sinon, la méthode effectue un vrai POST /api/auth/login.
    /// </summary>
    public static async Task<string> GetTokenAsync(
        HttpClient client,
        string email,
        string password = TestUsers.DefaultPassword)
    {
        var loginResponse = await client.PostAsJsonAsync(
            "/api/auth/login",
            new
            {
                email,
                password
            });

        var responseBody = await loginResponse.Content.ReadAsStringAsync();

        loginResponse.StatusCode.Should().Be(
            HttpStatusCode.OK,
            $"Login échoué pour {email}. Réponse API : {responseBody}");

        var auth = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();

        auth.Should().NotBeNull();

        auth!.AccessToken.Should()
            .NotBeNullOrWhiteSpace();

        return auth.AccessToken;
    }

    /// <summary>
    /// Représente uniquement la réponse JSON retournée par /api/auth/login.
    ///
    /// privé car ne sert qu'au helper et sealed car elle n'a pas vocation à être héritée.
    /// </summary>
    private sealed class AuthResponse
    {
        public string AccessToken { get; set; } = string.Empty;

        public string RefreshToken { get; set; } = string.Empty;

        public DateTime AccessTokenExpiresAt { get; set; }
    }
}