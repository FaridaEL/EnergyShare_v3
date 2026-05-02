using EnergyShare_v3.Domain.Entities.Messages;
using FluentAssertions;

namespace EnergyShare_v3.Backend.Tests
{
    public class MessageTests
    {
        [Fact]
        public void Create_ShouldSucceed_WhenDataIsValid()
        {
            // Arrange
            var expediteurId = Guid.NewGuid();
            var destinataireId = Guid.NewGuid();
            var matchId = Guid.NewGuid();

            // Act
            var result = Message.Create(
                objet: "Demande de contact",
                contenu: "Bonjour, je suis intéressé par votre profil.",
                expediteurId: expediteurId,
                destinataireId: destinataireId,
                matchId: matchId);

            // Assert
            result.IsSuccess.Should().BeTrue();

            var message = result.Value;
            message.ObjetMessage.Should().Be("Demande de contact");
            message.Contenu.Should().Be("Bonjour, je suis intéressé par votre profil.");
            message.ExpediteurId.Should().Be(expediteurId);
            message.DestinataireId.Should().Be(destinataireId);
            message.MatchId.Should().Be(matchId);
            message.IsLu.Should().BeFalse();
            message.DateEnvoi.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public void Create_ShouldFail_WhenExpediteurIdIsEmpty()
        {
            var result = Message.Create(
                objet: "Test",
                contenu: "Contenu",
                expediteurId: Guid.Empty,
                destinataireId: Guid.NewGuid());

            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public void Create_ShouldFail_WhenDestinataireIdIsEmpty()
        {
            var result = Message.Create(
                objet: "Test",
                contenu: "Contenu",
                expediteurId: Guid.NewGuid(),
                destinataireId: Guid.Empty);

            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public void Create_ShouldFail_WhenExpediteurAndDestinataireAreSame()
        {
            var userId = Guid.NewGuid();

            var result = Message.Create(
                objet: "Test",
                contenu: "Contenu",
                expediteurId: userId,
                destinataireId: userId);

            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public void Create_ShouldFail_WhenObjetIsMissing()
        {
            var result = Message.Create(
                objet: "",
                contenu: "Contenu",
                expediteurId: Guid.NewGuid(),
                destinataireId: Guid.NewGuid());

            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public void Create_ShouldFail_WhenContenuIsMissing()
        {
            var result = Message.Create(
                objet: "Test",
                contenu: "",
                expediteurId: Guid.NewGuid(),
                destinataireId: Guid.NewGuid());

            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public void MarquerCommeLu_ShouldSetIsLuToTrue()
        {
            var message = Message.Create(
                objet: "Test",
                contenu: "Contenu",
                expediteurId: Guid.NewGuid(),
                destinataireId: Guid.NewGuid()).Value;

            message.MarquerCommeLu();

            message.IsLu.Should().BeTrue();
        }

        [Fact]
        public void MarquerCommeNonLu_ShouldSetIsLuToFalse()
        {
            var message = Message.Create(
                objet: "Test",
                contenu: "Contenu",
                expediteurId: Guid.NewGuid(),
                destinataireId: Guid.NewGuid()).Value;

            message.MarquerCommeLu();
            message.MarquerCommeNonLu();

            message.IsLu.Should().BeFalse();
        }
    }
}