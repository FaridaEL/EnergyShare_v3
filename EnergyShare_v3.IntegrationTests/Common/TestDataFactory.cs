using EnergyShare_v3.Application.Features.PointAccess;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace EnergyShare_v3.IntegrationTests.Common;

/// <summary>
/// Fabrique de données de test pour les tests d'intégration.
///
/// Cette classe crée des données propres à chaque test :
/// - utilisateur unique ;
/// - email unique ;
/// - EAN unique ;
/// - compteur unique ;
/// - point d'accès actif.
///
/// Objectif principal : éviter que plusieurs tests utilisent le même utilisateur seedé
/// et se bloquent mutuellement à cause des règles métier, par exemple :
/// 1 EAN / point d'accès = 1 partage non clôturé.
/// </summary>
public class TestDataFactory
{
    private readonly HttpClient _client;

    public TestDataFactory(HttpClient client)
    {
        _client = client;
    }

    /// <summary>
    /// Crée un utilisateur vendeur de test avec un point d'accès actif d'injection.
    ///
    /// Cette méthode effectue un vrai parcours API :
    /// 1. création du compte via /api/auth/register ;
    /// 2. authentification via TestAuthHelper ;
    /// 3. création d'un point d'accès actif d'injection via /api/points-access.
    ///
    /// Après l'appel à cette méthode, le HttpClient est déjà authentifié
    /// avec l'utilisateur créé.
    ///
    /// Elle retourne l'email au cas où le test en aurait besoin.
    /// </summary>
    public async Task<string> CreateSellerWithInjectionPointAsync()
    {
        var email = GenerateUniqueEmail("seller");
        var ean = GenerateUniqueEan();
        var smartMeter = GenerateUniqueSmartMeter();

        var registerResponse = await _client.PostAsJsonAsync(
            "/api/auth/register",
            new
            {
                email,
                password = TestUsers.DefaultPassword,
                firstName = "Seller",
                lastName = "Integration"
            });

        var registerBody = await registerResponse.Content.ReadAsStringAsync();

        registerResponse.StatusCode.Should().Be(
            HttpStatusCode.OK,
            $"Création utilisateur test échouée. Réponse API : {registerBody}");

        await TestAuthHelper.AuthenticateAsync(_client, email);

        var createPointCommand = new CreatePointAccess(
            AdresseLine1: "Rue Test Integration 1",
            CodePostal: "1000",
            Fournisseur: "Engie",
            SmartMeter: smartMeter,
            EAN: ean,
            IsInjectionPoint: true);

        var pointAccessResponse = await _client.PostAsJsonAsync(
            "/api/points-access",
            createPointCommand);

        var pointAccessBody = await pointAccessResponse.Content.ReadAsStringAsync();



        if (pointAccessResponse.StatusCode != HttpStatusCode.OK &&
           pointAccessResponse.StatusCode != HttpStatusCode.Created)
                {
                    throw new Exception(
                        $"Création point d'accès test échouée. Réponse API : {pointAccessBody}");
                }

        pointAccessResponse.StatusCode.Should().BeOneOf(
          HttpStatusCode.OK,
          HttpStatusCode.Created);



        return email;
    }





    /// <summary>
    /// Crée un vendeur de test avec un point d'accès actif d'injection
    /// et retourne à la fois :
    /// - l'email du vendeur ;
    /// - le vrai PointAccessId créé.
    ///
    /// Cette méthode est utilisée par les tests qui doivent réellement
    /// créer un partage avec un PointAccessId valide.
    /// </summary>
    public async Task<(string Email, Guid PointAccessId)>
        CreateSellerWithInjectionPointDataAsync()
    {
        var email = GenerateUniqueEmail("seller");
        var ean = GenerateUniqueEan();
        var smartMeter = GenerateUniqueSmartMeter();

        // 1. Création du compte vendeur
        var registerResponse = await _client.PostAsJsonAsync(
            "/api/auth/register",
            new
            {
                email,
                password = TestUsers.DefaultPassword,
                firstName = "Seller",
                lastName = "Integration"
            });

        var registerBody =
            await registerResponse.Content.ReadAsStringAsync();

        registerResponse.StatusCode.Should().Be(
            HttpStatusCode.OK,
            $"Création utilisateur test échouée. Réponse API : {registerBody}");

        // 2. Authentification
        await TestAuthHelper.AuthenticateAsync(
            _client,
            email);

        // 3. Création du point d'accès injecteur
        var createPointCommand = new CreatePointAccess(
            AdresseLine1: "Rue Test Integration 1",
            CodePostal: "1000",
            Fournisseur: "Engie",
            SmartMeter: smartMeter,
            EAN: ean,
            IsInjectionPoint: true);

        var pointAccessResponse = await _client.PostAsJsonAsync(
            "/api/points-access",
            createPointCommand);

        var pointAccessBody =
            await pointAccessResponse.Content.ReadAsStringAsync();

        if (pointAccessResponse.StatusCode != HttpStatusCode.OK &&
            pointAccessResponse.StatusCode != HttpStatusCode.Created)
        {
            throw new Exception(
                $"Création point d'accès test échouée. Réponse API : {pointAccessBody}");
        }

        // 4. Récupération du vrai Id du point créé
        var pointAccessId = await pointAccessResponse.Content
            .ReadFromJsonAsync<Guid>();

        pointAccessId.Should().NotBeEmpty();

        // 5. Retour email + PointAccessId
        return (
            Email: email,
            PointAccessId: pointAccessId);
    }
    /// <summary>
    /// Crée un acheteur avec un point de consommation.
    ///
    /// Utilisé pour les scénarios où un utilisateur doit rejoindre
    /// un partage existant.
    /// </summary>
    public async Task<string> CreateBuyerWithConsumptionPointAsync()
    {
        var email = GenerateUniqueEmail("buyer");
        var ean = GenerateUniqueEan();
        var smartMeter = GenerateUniqueSmartMeter();

        var registerResponse = await _client.PostAsJsonAsync(
            "/api/auth/register",
            new
            {
                email,
                password = TestUsers.DefaultPassword,
                firstName = "Buyer",
                lastName = "Integration"
            });

        var registerBody =
            await registerResponse.Content.ReadAsStringAsync();

        registerResponse.StatusCode.Should().Be(
            HttpStatusCode.OK,
            $"Création utilisateur test échouée. Réponse API : {registerBody}");

        await TestAuthHelper.AuthenticateAsync(_client, email);

        var createPointCommand = new CreatePointAccess(
            AdresseLine1: "Rue Acheteur Integration 1",
            CodePostal: "1000",
            Fournisseur: "Mega",
            SmartMeter: smartMeter,
            EAN: ean,
            IsInjectionPoint: false);

        var pointAccessResponse = await _client.PostAsJsonAsync(
            "/api/points-access",
            createPointCommand);

        var pointAccessBody =
            await pointAccessResponse.Content.ReadAsStringAsync();

        pointAccessResponse.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Created);

        return email;
    }
    /// <summary>
    /// Génère un email unique.
    ///
    /// Exemple :
    /// seller.4f7a9d...@integration.test
    /// </summary>
    public static string GenerateUniqueEmail(string prefix = "user")
    {
        return $"{prefix}.{Guid.NewGuid():N}@integration.test";
    }

    /// <summary>
    /// Génère un EAN unique et valide pour les tests.
    ///
    /// Règle métier :
    /// un EAN commence par 5414489 et comporte 18 chiffres.
    /// </summary>
    public static string GenerateUniqueEan()
    {
        var suffix = Random.Shared.NextInt64(
            10_000_000_000,
            99_999_999_999);

        return $"5414489{suffix}";
    }

    /// <summary>
    /// Génère un numéro de compteur intelligent unique.
    ///
    /// Règle métier :
    /// un compteur intelligent commence par 1SJ.
    /// </summary>
    public static string GenerateUniqueSmartMeter()
    {
        var suffix = Guid.NewGuid()
            .ToString("N")[..8]
            .ToUpperInvariant();

        return $"1SJ{suffix}";
    }
}