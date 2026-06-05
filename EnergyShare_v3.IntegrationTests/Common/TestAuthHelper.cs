using FluentAssertions;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace EnergyShare_v3.IntegrationTests.Common;

/// <summary>
/// Helper commun pour l'authentification dans les tests d'intégration.
///
/// Cette classe effectue un vrai appel HTTP vers l'endpoint /api/auth/login, comme le ferait
/// un utilisateur réel via l'interface.
///
/// Objectifs :
/// - éviter de dupliquer le code de login dans chaque classe de test ;
/// - récupérer un token JWT valide ;
/// - ajouter automatiquement ce token dans le HttpClient ;
/// - mettre les tokens en cache pour éviter les appels répétés à /api/auth/login,
///   notamment à cause du rate limiter qui peut renvoyer une erreur 429.
/// </summary>
public static class TestAuthHelper
{
    /// <summary>
    /// Cache des tokens par email.
    ///
    /// Clé   : email de l'utilisateur, par exemple sarah.dupont@example.com
    /// Valeur: token JWT retourné par l'API.
    ///
    /// Sans cache, chaque test referait un login complet.
    /// Avec le cache, le premier login récupère le token, puis les tests suivants
    /// réutilisent le même token pour le même utilisateur.
    /// </summary>
    private static readonly Dictionary<string, string> TokenCache = new();

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
        if (TokenCache.TryGetValue(email, out var cachedToken))
        {
            return cachedToken;
        }

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

        TokenCache[email] = auth.AccessToken;

        return auth.AccessToken;
    }

    /// <summary>
    /// Représente uniquement la réponse JSON retournée par /api/auth/login.
    ///
    /// Cette classe est privée car elle ne sert qu'au helper.
    /// Elle est sealed car elle n'a pas vocation à être héritée.
    /// </summary>
    private sealed class AuthResponse
    {
        public string AccessToken { get; set; } = string.Empty;

        public string RefreshToken { get; set; } = string.Empty;

        public DateTime AccessTokenExpiresAt { get; set; }
    }
}