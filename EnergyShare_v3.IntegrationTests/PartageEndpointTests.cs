using EnergyShare_v3.Application.Features.Partage;
using EnergyShare_v3.Domain.Enums;
using EnergyShare_v3.Web.Models.Partage;
using FluentAssertions;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using static EnergyShare_v3.Web.Endpoints.PartageEndpoint;

namespace EnergyShare_v3.IntegrationTests
{
    public class PartageEndpointTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private static string? _sarahToken;
        private static string? _julienToken;
        private static string? _adminToken;
        private static string? _leaToken;

        public PartageEndpointTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task CreatePartage_WithoutAuthentication_ShouldReturnUnauthorized()
        {
            var command = new CreatePartage(
                Nom: "Partage Test Integration",
                EnergieType: PartageEnergieType.PairToPair);

            var response = await _client.PostAsJsonAsync("/api/partages", command);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task CreatePartage_WithSarahToken_ShouldReturnCreated()
        {
            await AuthenticateAsync("sarah.dupont@example.com");

            var command = new CreatePartage(
                Nom: "Partage Test Integration",
                EnergieType: PartageEnergieType.PairToPair);

            var response = await _client.PostAsJsonAsync("/api/partages", command);

            response.StatusCode.Should().Be(HttpStatusCode.Created);

            var partageId = await response.Content.ReadFromJsonAsync<Guid>();
            partageId.Should().NotBeEmpty();
        }

        [Fact]
        public async Task GetPartageById_WhenUserIsSeller_ShouldReturnOk()
        {
            await AuthenticateAsync("sarah.dupont@example.com");

            var command = new CreatePartage(
                Nom: "Partage Seller Access Test",
                EnergieType: PartageEnergieType.PairToPair);

            var createResponse = await _client.PostAsJsonAsync("/api/partages", command);
            createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            var partageId = await createResponse.Content.ReadFromJsonAsync<Guid>();

            var response = await _client.GetAsync($"/api/partages/{partageId}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var dto = await response.Content.ReadFromJsonAsync<PartageDetailsDto>();
            dto.Should().NotBeNull();
            dto!.Id.Should().Be(partageId);
            dto.Nom.Should().Be("Partage Seller Access Test");
            //  on attend a minima 1 participant (le créateur) 
            dto.NombreParticipants.Should().Be(1);
        }

        [Fact]
        public async Task GetPartages_WithoutAuthentication_ShouldReturnUnauthorized()
        {
            var response = await _client.GetAsync("/api/partages");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetPartages_WhenUserIsNotAdmin_ShouldReturnForbidden()
        {
            await AuthenticateAsync("julien.martin@example.com"); 

            var response = await _client.GetAsync("/api/partages");

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetPartages_WhenUserIsAdmin_ShouldReturnOk()
        {
            await AuthenticateAsync("admin.test@example.com");

            var response = await _client.GetAsync("/api/partages");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var partages = await response.Content.ReadFromJsonAsync<List<PartageSummaryDto>>();
            partages.Should().NotBeNull();
        }


        [Fact]
        public async Task UpdatePartage_WithoutAuthentication_ShouldReturnUnauthorized()
        {
            // Arrange
            var request = new UpdatePartageRequest(
                Nom: "Update interdit",
                Description: "Non authentifié",
                EnergieType: PartageEnergieType.PairToPair,
                DateDebut: new DateTime(2026, 6, 1),
                DateFin: null);

            // Act
            var response = await _client.PutAsJsonAsync(
                $"/api/partages/{Guid.NewGuid()}",
                request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task UpdatePartage_WhenUserIsSeller_ShouldReturnSuccess()
        {
            // Arrange
            await AuthenticateAsync("sarah.dupont@example.com");

            var createCommand = new CreatePartage(
                Nom: "Partage à modifier",
                EnergieType: PartageEnergieType.PairToPair);

            var createResponse = await _client.PostAsJsonAsync("/api/partages", createCommand);
            createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            var partageId = await createResponse.Content.ReadFromJsonAsync<Guid>();

            var request = new UpdatePartageRequest(
                Nom: "Partage modifié integration",
                Description: "Description modifiée depuis test intégration",
                EnergieType: PartageEnergieType.MemeBatiment,
                DateDebut: new DateTime(2026, 6, 1),
                DateFin: new DateTime(2026, 12, 31));

            // Act
            var response = await _client.PutAsJsonAsync(
                $"/api/partages/{partageId}",
                request);

            // Assert
            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.OK,
                HttpStatusCode.NoContent);

            var getResponse = await _client.GetAsync($"/api/partages/{partageId}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var dto = await getResponse.Content.ReadFromJsonAsync<PartageDetailsDto>();

            dto.Should().NotBeNull();
            dto!.Nom.Should().Be("Partage modifié integration");
            dto.Description.Should().Be("Description modifiée depuis test intégration");
            dto.EnergieType.Should().Be(PartageEnergieType.MemeBatiment);
            dto.DateDebut.Should().Be(new DateTime(2026, 6, 1));
            dto.DateFin.Should().Be(new DateTime(2026, 12, 31));
            dto.UpdatedAt.Should().NotBe(default(DateTime));
        }

        [Fact]
        public async Task UpdatePartage_WhenUserIsNotSeller_ShouldReturnForbidden()
        {
            // Arrange : Sarah crée un partage.
            await AuthenticateAsync("sarah.dupont@example.com");

            var createCommand = new CreatePartage(
                Nom: "Partage Sarah sécurisé",
                EnergieType: PartageEnergieType.PairToPair);

            var createResponse = await _client.PostAsJsonAsync("/api/partages", createCommand);
            createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            var partageId = await createResponse.Content.ReadFromJsonAsync<Guid>();

            // Julien tente de modifier le partage de Sarah.
            await AuthenticateAsync("julien.martin@example.com");

            var request = new UpdatePartageRequest(
                Nom: "Modification interdite",
                Description: "Julien ne peut pas modifier",
                EnergieType: PartageEnergieType.MemeBatiment,
                DateDebut: new DateTime(2026, 6, 1),
                DateFin: null);

            // Act
            var response = await _client.PutAsJsonAsync(
                $"/api/partages/{partageId}",
                request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task UpdatePartage_WhenDateFinIsBeforeDateDebut_ShouldReturnBadRequest()
        {
            // Arrange
            await AuthenticateAsync("sarah.dupont@example.com");

            var createCommand = new CreatePartage(
                Nom: "Partage validation dates",
                EnergieType: PartageEnergieType.PairToPair);

            var createResponse = await _client.PostAsJsonAsync("/api/partages", createCommand);
            createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            var partageId = await createResponse.Content.ReadFromJsonAsync<Guid>();

            var request = new UpdatePartageRequest(
                Nom: "Partage validation dates",
                Description: "Dates invalides",
                EnergieType: PartageEnergieType.PairToPair,
                DateDebut: new DateTime(2026, 12, 31),
                DateFin: new DateTime(2026, 6, 1));

            // Act
            var response = await _client.PutAsJsonAsync(
                $"/api/partages/{partageId}",
                request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
        //Test sur l'ajout d'utilisateur via invitation par code
       
        //test pour le créateur :

        [Fact]
        public async Task GetInvitationCodePartage_WhenUserIsSeller_ShouldReturnOk()
        {
            // Arrange
            await AuthenticateAsync("sarah.dupont@example.com");

            var createCommand = new CreatePartage(
                Nom: "Partage invitation code",
                EnergieType: PartageEnergieType.PairToPair);

            var createResponse = await _client.PostAsJsonAsync("/api/partages", createCommand);
            createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            var partageId = await createResponse.Content.ReadFromJsonAsync<Guid>();

            // Act
            var response = await _client.PostAsync(
                $"/api/partages/{partageId}/invitation-code",
                content: null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var dto = await response.Content.ReadFromJsonAsync<InvitationCodeDto>();

            dto.Should().NotBeNull();
            dto!.PartageId.Should().Be(partageId);
            dto.InvitationCode.Should().NotBeNullOrWhiteSpace();
            dto.InvitationCode.Should().HaveLength(12);
            dto.InvitationCodeExpiresAt.Should().BeAfter(DateTime.UtcNow);
        }

        //test pour l’accès interdit :

        [Fact]
        public async Task GetInvitationCodePartage_WhenUserIsNotSeller_ShouldReturnForbidden()
        {
            // Arrange : Sarah crée un partage.
            await AuthenticateAsync("sarah.dupont@example.com");

            var createCommand = new CreatePartage(
                Nom: "Partage invitation interdit",
                EnergieType: PartageEnergieType.PairToPair);

            var createResponse = await _client.PostAsJsonAsync("/api/partages", createCommand);
            createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            var partageId = await createResponse.Content.ReadFromJsonAsync<Guid>();

            // Act : Julien tente de récupérer le code.
            await AuthenticateAsync("julien.martin@example.com");

            var response = await _client.PostAsync(
                $"/api/partages/{partageId}/invitation-code",
                content: null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        //test pour rejoindre :
        [Fact]
        public async Task RejoindrePartage_WithValidInvitationCode_ShouldReturnOk()
        {
            // Arrange : Sarah crée un partage
            await AuthenticateAsync("sarah.dupont@example.com");

            var createCommand = new CreatePartage(
                Nom: "Partage à rejoindre",
                EnergieType: PartageEnergieType.PairToPair);

            var createResponse = await _client.PostAsJsonAsync("/api/partages", createCommand);
            createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            var partageId = await createResponse.Content.ReadFromJsonAsync<Guid>();

            // Génération du code d’invitation
            var invitationResponse = await _client.PostAsync(
                $"/api/partages/{partageId}/invitation-code",
                content: null);

            invitationResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var invitationDto = await invitationResponse.Content.ReadFromJsonAsync<InvitationCodeDto>();
            invitationDto.Should().NotBeNull();

            // Act : Léa rejoint le partage
            await AuthenticateAsync("lea.bernard@example.com");

            var response = await _client.PostAsJsonAsync(
                "/api/partages/rejoindre",
                new RejoindrePartageRequest(invitationDto!.InvitationCode));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var joinedPartageId = await response.Content.ReadFromJsonAsync<Guid>();
            joinedPartageId.Should().Be(partageId);
        }
        //N'est pas déjà dans un partage actif 
        //[Fact]
        //public async Task RejoindrePartage_WhenAlreadyInActivePartage_ShouldReturnBadRequest()

        //code invalide
        [Fact]
        public async Task RejoindrePartage_WithInvalidCode_ShouldReturnBadRequest()
        {
            await AuthenticateAsync("lea.bernard@example.com");

            var response = await _client.PostAsJsonAsync(
                "/api/partages/rejoindre",
                new RejoindrePartageRequest("CODEINCONNU"));

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        //Sans authentification
        [Fact]
        public async Task RejoindrePartage_WithoutAuthentication_ShouldReturnUnauthorized()
        {
            _client.DefaultRequestHeaders.Authorization = null;

            var response = await _client.PostAsJsonAsync(
                "/api/partages/rejoindre",
                new RejoindrePartageRequest("ABC123456789"));

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        //TEst pour demander les infos de périmètre au GRD :
        [Fact]
        public async Task DemandeInfoPerimetrePartage_WhenSellerAndPartageComplete_ShouldReturnOk()
        {
            // Arrange : Sarah crée un partage.
            await AuthenticateAsync("sarah.dupont@example.com");

            var createResponse = await _client.PostAsJsonAsync(
                "/api/partages",
                new CreatePartage("Partage demande périmètre", PartageEnergieType.PairToPair));

            createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            var partageId = await createResponse.Content.ReadFromJsonAsync<Guid>();

            // Sarah génère un code d’invitation.
            var invitationResponse = await _client.PostAsync(
                $"/api/partages/{partageId}/invitation-code",
                null);

            invitationResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var invitationDto = await invitationResponse.Content.ReadFromJsonAsync<InvitationCodeDto>();
            invitationDto.Should().NotBeNull();

            // Léa rejoint le partage pour atteindre 2 participants.
            await AuthenticateAsync("lea.bernard@example.com");

            var joinResponse = await _client.PostAsJsonAsync(
                "/api/partages/rejoindre",
                new RejoindrePartageRequest(invitationDto!.InvitationCode));

            joinResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // Act : Sarah demande les infos de périmètre.
            await AuthenticateAsync("sarah.dupont@example.com");

            var response = await _client.PostAsync(
                $"/api/partages/{partageId}/demande-info-perimetre",
                null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var dto = await response.Content.ReadFromJsonAsync<DemandePerimetreDto>();

            dto.Should().NotBeNull();
            dto!.PartageId.Should().Be(partageId);
            dto.DemandeId.Should().NotBeEmpty();
            dto.ResponseStatus.Should().Be(DdeGRDResponseStatus.EnAttente.ToString());
            dto.DetailsDemande.Should().Contain("Adresses des points d’accès concernés");
        }


        //authentification

        private async Task AuthenticateAsync(string email, string password = "Test1234")
        {   
        // erreur 429 --> trop de tenative de connexion -> solution passer en cache:
        var token = email switch
            {
                "sarah.dupont@example.com" =>
                    _sarahToken ??= await GetTokenAsync(email, password),

                "julien.martin@example.com" =>
                    _julienToken ??= await GetTokenAsync(email, password),

                "admin.test@example.com" =>
                    _adminToken ??= await GetTokenAsync(email, password),

                "lea.bernard@example.com" =>
                    _leaToken ??= await GetTokenAsync(email, password),

                _ => await GetTokenAsync(email, password)
            };


            //var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new
            //{
            //    email,
            //    password
            //});

            //loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            //var auth = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
            //auth.Should().NotBeNull();
            //auth!.AccessToken.Should().NotBeNullOrWhiteSpace();

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }


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


        private sealed class AuthResponse
        {
            public string AccessToken { get; set; } = string.Empty;
            public string RefreshToken { get; set; } = string.Empty;
            public DateTime AccessTokenExpiresAt { get; set; }
        }



        private async Task<string> CreateInvitationCodeAsync()
        {
            await AuthenticateAsync("sarah.dupont@example.com");

            var createResponse = await _client.PostAsJsonAsync("/api/partages",
                new CreatePartage("Test", PartageEnergieType.PairToPair));

            var partageId = await createResponse.Content.ReadFromJsonAsync<Guid>();

            var invitationResponse = await _client.PostAsync(
                $"/api/partages/{partageId}/invitation-code", null);

            var dto = await invitationResponse.Content.ReadFromJsonAsync<InvitationCodeDto>();

            return dto!.InvitationCode;
        }
    }
}