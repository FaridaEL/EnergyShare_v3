using EnergyShare_v3.Domain.Entities.ProfilsEnergie;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnergyShare_v3.Backend.Tests
{
    public class ProfilEnergieTests
    {
        [Fact]
        public void Create_ShouldFail_WhenNeitherDemandNorOfferIsProvided()
        {
            // Arrange
            var pointAccessId = Guid.NewGuid();

            // Act
            var result = ProfilEnergie.Create(
                demande: null,
                offre: null,
                prixAchatCible: 0.15m,
                prixVenteCible: null,
                pointAccessId: pointAccessId);

            // Assert
            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public void Create_ShouldFail_WhenDemandIsNegative()
        {
            // Arrange
            var pointAccessId = Guid.NewGuid();

            // Act
            var result = ProfilEnergie.Create(
                demande: -10m,
                offre: null,
                prixAchatCible: 0.15m,
                prixVenteCible: null,
                pointAccessId: pointAccessId);

            // Assert
            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public void Create_ShouldSucceed_WhenDataIsValid()
        {
            // Arrange
            var pointAccessId = Guid.NewGuid();

            // Act
            var result = ProfilEnergie.Create(
                demande: 1200m,
                offre: null,
                prixAchatCible: 0.16m,
                prixVenteCible: null,
                pointAccessId: pointAccessId);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.PointAccessId.Should().Be(pointAccessId);
            result.Value.DemandeEnergie_kWh.Should().Be(1200m);
            result.Value.OffreEnergie_kWh.Should().BeNull();
            result.Value.PrixAchatCible_Eur.Should().Be(0.16m);
        }
    }
}
