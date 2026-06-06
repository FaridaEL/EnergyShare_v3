namespace EnergyShare_v3.IntegrationTests.Common;

/// <summary>
/// Utilisateurs de référence créés par le seeder d'intégration.
/// Cette classe évite la duplication des emails dans les tests.
/// </summary>
public static class TestUsers
{
    /// <summary>
    /// Mot de passe utilisé par tous les utilisateurs seedés.
    /// A modifier ici uniquement si la politique de sécurité évolue.
    /// </summary>
    public const string DefaultPassword = "Test1234!";

    // Administrateur
    public const string Admin = "admin.test@example.com";

    // GRD
    public const string AgentSibelga = "agent.sibelga@example.com";

    // Vendeurs
    public const string Sarah = "sarah.dupont@example.com";
    public const string Julien = "julien.martin@example.com";

    // Acheteurs
    public const string Lea = "lea.bernard@example.com";
    public const string Hugo = "hugo.lambert@example.com";

    // Société (acheteurs)
    public const string Boulangerie = "contact@boulangerie-dupain.be";
}