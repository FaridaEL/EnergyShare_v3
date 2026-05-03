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


    }
}
