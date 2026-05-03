using EnergyShare_v3.Domain.Entities.Partages;
using EnergyShare_v3.Domain.Enums;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnergyShare_v3.Backend.Tests
{
    public class PartageTests
    {
        [Fact]
        public void Create_ShouldFail_WhenNameIsEmpty()
        {
            // Arrange
            var vendeurId = Guid.NewGuid();

            // Act
            var result = Partage.Create(
                nom: "",
                energieType: PartageEnergieType.PairToPair,
                vendeurId: vendeurId);

            // Assert
            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public void Create_ShouldFail_WhenSellerIdIsEmpty()
        {
            // Arrange
            var vendeurId = Guid.Empty;

            // Act
            var result = Partage.Create(
                nom: "Partage Test",
                energieType: PartageEnergieType.PairToPair,
                vendeurId: vendeurId);

            // Assert
            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public void Create_ShouldSucceed_WhenDataIsValid()
        {
            // Arrange
            var vendeurId = Guid.NewGuid();

            // Act
            var result = Partage.Create(
                nom: "Partage Test",
                energieType: PartageEnergieType.PairToPair,
                vendeurId: vendeurId);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Nom.Should().Be("Partage Test");
            result.Value.VendeurId.Should().Be(vendeurId);
            result.Value.EnergieType.Should().Be(PartageEnergieType.PairToPair);
            result.Value.Statut.Should().Be(PartageEnergieStatutType.Inactif);
        }

        [Fact]
        public void AjouterMembre_ShouldSucceed_EvenIfPairToPairHasOnlyOneMember()
        {
            // Arrange
            var vendeurId = Guid.NewGuid();

            var partage = Partage.Create(
                "Partage Test",
                PartageEnergieType.PairToPair,
                vendeurId).Value;

            var membre = new ParticipationPartage
            {
                Id = Guid.NewGuid(),
                PartageId = partage.Id,
                PointAccessId = Guid.NewGuid(),
                UserRolePartage = UserRolePartage.Acheteur,
                JoinedAt = DateTime.UtcNow
            };

            // Act
            var result = partage.AjouterMembre(membre);

            // Assert
            result.IsSuccess.Should().BeTrue();
            partage.Membres.Should().HaveCount(1);
        }

        [Fact]
        public void SoumettreNouveauPartageAuGrd_ShouldFail_WhenPairToPairHasOnlyOneMember()
        {
            // Arrange
            var vendeurId = Guid.NewGuid();

            var partage = Partage.Create(
                "Partage Test",
                PartageEnergieType.PairToPair,
                vendeurId).Value;

            var membre = new ParticipationPartage
            {
                Id = Guid.NewGuid(),
                PartageId = partage.Id,
                PointAccessId = Guid.NewGuid(),
                UserRolePartage = UserRolePartage.Acheteur,
                JoinedAt = DateTime.UtcNow
            };

            partage.AjouterMembre(membre);

            // Act
            var result = partage.SoumettreNouveauPartageAuGrd();

            // Assert
            result.IsSuccess.Should().BeFalse();
            partage.Statut.Should().Be(PartageEnergieStatutType.Inactif);
        }

        [Fact]
        public void Update_ShouldSucceed_WhenPartageIsInactif()
        {
            // Arrange
            var vendeurId = Guid.NewGuid();

            var partage = Partage.Create(
                "Partage initial",
                PartageEnergieType.PairToPair,
                vendeurId).Value;

            var dateDebut = new DateTime(2026, 6, 1);
            var dateFin = new DateTime(2026, 12, 31);

            // Act
            var result = partage.Update(
                "Partage modifié",
                "Description modifiée",
                PartageEnergieType.MemeBatiment,
                dateDebut,
                dateFin);

            // Assert
            result.IsSuccess.Should().BeTrue();
            partage.Nom.Should().Be("Partage modifié");
            partage.Description.Should().Be("Description modifiée");
            partage.EnergieType.Should().Be(PartageEnergieType.MemeBatiment);
            partage.DateDebut.Should().Be(dateDebut);
            partage.DateFin.Should().Be(dateFin);
        }

        [Fact]
        public void Update_ShouldFail_WhenNameIsEmpty()
        {
            // Arrange
            var vendeurId = Guid.NewGuid();

            var partage = Partage.Create(
                "Partage initial",
                PartageEnergieType.PairToPair,
                vendeurId).Value;

            // Act
            var result = partage.Update(
                "",
                "Description",
                PartageEnergieType.PairToPair,
                null,
                null);

            // Assert
            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public void Update_ShouldFail_WhenDateFinIsBeforeDateDebut()
        {
            // Arrange
            var vendeurId = Guid.NewGuid();

            var partage = Partage.Create(
                "Partage initial",
                PartageEnergieType.PairToPair,
                vendeurId).Value;

            // Act
            var result = partage.Update(
                "Partage test",
                "Description",
                PartageEnergieType.PairToPair,
                new DateTime(2026, 12, 31),
                new DateTime(2026, 6, 1));

            // Assert
            result.IsSuccess.Should().BeFalse();
        }

        //Test sur l'ajout d'utilisateur via invitation par code
        [Fact]
        public void Create_ShouldNotGenerateInvitationCode()
        {
            // Arrange
            var vendeurId = Guid.NewGuid();

            // Act
            var result = Partage.Create(
                nom: "Partage Test",
                energieType: PartageEnergieType.PairToPair,
                vendeurId: vendeurId);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.InvitationCode.Should().BeNull();
            result.Value.InvitationCodeExpiresAt.Should().BeNull();
        }

        [Fact]
        public void EnsureValidInvitationCode_ShouldGenerateCode_WhenNoCodeExists()
        {
            // Arrange
            var partage = Partage.Create(
                "Partage Test",
                PartageEnergieType.PairToPair,
                Guid.NewGuid()).Value;

            // Act
            var result = partage.EnsureValidInvitationCode();

            // Assert
            result.IsSuccess.Should().BeTrue();
            partage.InvitationCode.Should().NotBeNullOrWhiteSpace();
            partage.InvitationCode.Should().HaveLength(12);
            partage.InvitationCodeExpiresAt.Should().NotBeNull();
            partage.InvitationCodeExpiresAt!.Value.Should().BeAfter(DateTime.UtcNow);
        }

        [Fact]
        public void EnsureValidInvitationCode_ShouldKeepSameCode_WhenCodeIsStillValid()
        {
            // Arrange
            var partage = Partage.Create(
                "Partage Test",
                PartageEnergieType.PairToPair,
                Guid.NewGuid()).Value;

            partage.EnsureValidInvitationCode();

            var firstCode = partage.InvitationCode;
            var firstExpiration = partage.InvitationCodeExpiresAt;

            // Act
            var result = partage.EnsureValidInvitationCode();

            // Assert
            result.IsSuccess.Should().BeTrue();
            partage.InvitationCode.Should().Be(firstCode);
            partage.InvitationCodeExpiresAt.Should().Be(firstExpiration);
        }



    }
}
